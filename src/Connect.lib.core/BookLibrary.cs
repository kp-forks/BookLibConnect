﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using core.audiamus.aux;
using core.audiamus.aux.ex;
using core.audiamus.booksdb;
using core.audiamus.booksdb.ex;
using core.audiamus.connect.ex;
using Microsoft.EntityFrameworkCore;
using R = core.audiamus.connect.Properties.Resources;
using static core.audiamus.aux.Logging;

namespace core.audiamus.connect {
  class BookLibrary : IBookLibrary {
		const int PAGE_SIZE = 200;

		public readonly string _dbDir = null;
		public readonly string IMG_DIR = Path.Combine (ApplEnv.LocalApplDirectory, "img");

		public readonly Dictionary <ProfileId, IEnumerable<Book>> _bookCache = new Dictionary<ProfileId, IEnumerable<Book>>();

		public BookLibrary (string dbDir = null) => _dbDir = dbDir;

		public async Task<DateTime> SinceLatestPurchaseDateAsync () {
			return await Task.Run (() => sinceLatestPurchaseDate ());
		}

		public async Task AddBooksAsync (List<adb.json.Product> libProducts, ProfileId profileId) {
			using var _ = new LogGuard (3, this, () => $"#items={libProducts.Count}");
			await Task.Run (() => addBooks (libProducts, profileId));
			await Task.Run (() => cleanupDuplicateAuthors ());
		}

		public async Task AddCoverImagesAsync (Func<string, Task<byte[]>> downloadFunc) {
			using var _ = new LogGuard (3, this);

			Directory.CreateDirectory (IMG_DIR);
			
			using var dbContext = new BookDbContextLazyLoad (_dbDir);
			var files = Directory.GetFiles (IMG_DIR);

			var books = dbContext.Books
				.ToList ()
				.Where (c => c.CoverImageFile is null || !files.Contains (c.CoverImageFile))
				.ToList ();

			Log (3, this, () => $"#img={books.Count}");
			foreach (Book book in books) {
				Log (3, this, () => book.ToString());
				string url = book.CoverImageUrl;
				if (url is null)
					continue;
				byte[] img = await downloadFunc (url);
				if (img is null)
					continue;

				string ext = img.FindImageFormat ();
				if (ext is null)
					continue;
				string filename = $"{book.Asin}{ext}";
				string path = Path.Combine (IMG_DIR, filename);
				try {
					await File.WriteAllBytesAsync (path, img);

					book.CoverImageFile = path;
				} catch (Exception) { }
			}

			dbContext.SaveChanges ();
    }

		public IEnumerable<Book> GetBooks (ProfileId profileId) {
			using var _ = new LogGuard (3, this, () => profileId.ToString());

			lock (_bookCache) {
				bool succ = _bookCache.TryGetValue (profileId, out var cached);
				if (succ) {
					Log (3, this, () => $"from cache, #books={cached.Count()}");
					return cached;
				}
      }

			using var dbContext = new BookDbContext (_dbDir);
			//using var rg = new ResourceGuard (x => dbContext.ChangeTracker.LazyLoadingEnabled = !x);

			IEnumerable<Book> books = dbContext.Books
				.Include (b => b.Conversion)
				.Include (b => b.Components)
				.ThenInclude (c => c.Conversion)
				.Include (b => b.Authors)
				.Include (b => b.Narrators)
				.Include (b => b.Series)
				.ThenInclude (s => s.Series)
				.Include (b => b.Ladders)
				.ThenInclude (l => l.Rungs)
				.ThenInclude (r => r.Genre)
				.Include (b => b.Genres)
				.Include (b => b.Codecs)
				.ToList ();

			var booksByProfile = books
				.Where (b => b.Conversion.AccountId == profileId.AccountId && b.Conversion.Region == profileId.Region)
				.ToList ();

			lock (_bookCache)
				_bookCache [profileId] = booksByProfile;
			
			Log (3, this, () => $"from DB, #books={booksByProfile.Count ()}");

			return booksByProfile;
    }

		public IEnumerable<AccountAlias> GetAccountAliases () {
			using var _ = new LogGuard (3, this);
			using var dbContext = new BookDbContextLazyLoad (_dbDir);
			var accounts = dbContext.Accounts.ToList ();
			var contexts = accounts
				.Select (a => new AccountAlias (a.AudibleId, a.Alias))
				.ToList ();
			Log (3, this, () => $"#contexts={contexts.Count}");
			return contexts;
		}

    public AccountAliasContext GetAccountId (IProfile profile) {
			using var _ = new LogGuard (3, this);
			using var dbContext = new BookDbContextLazyLoad (_dbDir);

      string accountId = profile.CustomerInfo.AccountId;
      var account = dbContext.Accounts.FirstOrDefault (a => a.AudibleId == accountId);
      if (account is null) {
        List<uint> hashes = getAliasHashes ();
        account = new Account {
          AudibleId = accountId
        };
        dbContext.Accounts.Add (account);
        dbContext.SaveChanges ();
        return new AccountAliasContext (account.Id, profile.CustomerInfo.Name, hashes);
      } else {
        if (account.Alias.IsNullOrWhiteSpace ())
          return new AccountAliasContext (account.Id, profile.CustomerInfo.Name, getAliasHashes ());
        else
          return new AccountAliasContext (account.Id, null, null) {
            Alias = account.Alias
          };
      }

      List<uint> getAliasHashes () {
        return dbContext.Accounts
          .ToList ()
          .Where (a => !a.Alias.IsNullOrWhiteSpace ())
          .Select (a => a.Alias.Checksum32 ())
          .ToList ();
      }
    }


    public void SetAccountAlias (AccountAliasContext ctxt) {
			using var _ = new LogGuard (3, this);
			if (ctxt.Alias.IsNullOrWhiteSpace ())
				return;
			using var dbContext = new BookDbContextLazyLoad (_dbDir);
			var account = dbContext.Accounts.FirstOrDefault (a => a.Id == ctxt.LocalId);
			if (account is null)
				return;
			account.Alias = ctxt.Alias;
			dbContext.SaveChanges ();
    }

		public void SavePersistentState (Conversion conversion, EConversionState state) {
			using var _ = new LogGuard (4, this);
			using var dbContext = new BookDbContext (_dbDir);
			var conv = dbContext.Conversions.FirstOrDefault (c => conversion.Id == c.Id);
			if (conv is null)
				return;
			updateState (conv, state, conversion);
			dbContext.SaveChanges ();
    }

		public void RestorePersistentState (Conversion conversion) {
			using var _ = new LogGuard (4, this);
			using var dbContext = new BookDbContext (_dbDir);
			Conversion saved = dbContext.Conversions.FirstOrDefault (c => c.Id == conversion.Id);
			if (saved is not null)
				conversion.State = saved.State;
    }

		public EConversionState GetPersistentState (Conversion conversion) {
			using var _ = new LogGuard (4, this);
			using var dbContext = new BookDbContext (_dbDir);
			Conversion saved = dbContext.Conversions.FirstOrDefault (c => c.Id == conversion.Id);
			return saved?.State ?? EConversionState.unknown;
    }

		public void UpdateComponentProduct (IEnumerable<ProductComponentPair> componentPairs) {
			using var _ = new LogGuard (3, this);
			lock (this) {
				using var dbContext = new BookDbContext (_dbDir);
				foreach (var (item, comp) in componentPairs) {
					dbContext.Components.Attach (comp);
					comp.RunTimeLengthSeconds = item.runtime_length_min * 60;
					comp.Title = item.title;
				}
				dbContext.SaveChanges ();
			}
		}

		public void GetChapters (IBookCommon item) {
			using var _ = new LogGuard (3, this, () => item.ToString ());
			using var dbContext = new BookDbContext (_dbDir);
			if (item is Book book) {
				dbContext.Books.Attach (book);
				dbContext.Entry (book).Reference (b => b.ChapterInfo).Load ();
				dbContext.Entry (book.ChapterInfo).Collection (ci => ci.Chapters).Load ();
      } else if (item is Component comp) {
				dbContext.Components.Attach (comp);
				dbContext.Entry (comp).Reference (c => c.ChapterInfo).Load ();
				dbContext.Entry (comp.ChapterInfo).Collection (ci => ci.Chapters).Load ();
      }

			if (item.ChapterInfo.Chapters is List<Chapter> list)
				list.Sort ((x, y) => x.StartOffsetMs.CompareTo (y.StartOffsetMs));  
		}


		public void UpdateLicenseAndChapters (adb.json.ContentLicense license, Conversion conversion) {
			using var _ = new LogGuard (3, this, () => conversion.ToString ());
			using var dbContext = new BookDbContext (_dbDir);
      dbContext.Conversions.Attach (conversion);

			conversion.DownloadUrl = license.content_metadata.content_url.offline_url;

      var product = conversion.BookCommon;

			if (product is Component comp)
				dbContext.Components.Attach (comp);
			else if (product is Book book)
				dbContext.Books.Attach (book);

      var decryptedLic = license.decrypted_license_response;

      // Key and IV
      product.LicenseKey = decryptedLic?.key;
      product.LicenseIv = decryptedLic?.iv;

      setDownloadFilenameAndCodec (license, conversion);

			// file size
  		product.FileSizeBytes = license.content_metadata?.content_reference?.content_size_in_bytes;

			// duration
			int? runtime = license.content_metadata?.chapter_info?.runtime_length_sec;
			if (runtime.HasValue)
  			product.RunTimeLengthSeconds = runtime;

			// chapters
			addChapters (dbContext, license, conversion);

			updateState (conversion, EConversionState.license_granted);

      dbContext.SaveChanges ();
    }

		public void CheckUpdateFilesAndState (
			ProfileId profileId, 
			IDownloadSettings downloadSettings, 
			IExportSettings exportSettings, 
			Action<IConversion> callbackRefConversion
		) {
			using var lg = new LogGuard (3, this);
			using var dbContext = new BookDbContext (_dbDir);

			var conversions = dbContext.Conversions
				.ToList ();

			conversions = conversions
				.Where(c => c.AccountId == profileId.AccountId && c.Region == profileId.Region)
				.ToList ();

			var dnlddir = downloadSettings.DownloadDirectory;
			foreach (var conv in conversions) {
				var _ = conv.State switch {
					EConversionState.local_locked => checkLocalLocked (conv, callbackRefConversion, dnlddir),
					EConversionState.local_unlocked => checkLocalUnlocked (conv, callbackRefConversion, dnlddir),
					EConversionState.exported => checkExported (conv, callbackRefConversion, dnlddir, exportSettings?.ExportDirectory),
					EConversionState.converted => checkConverted (conv, callbackRefConversion, dnlddir),
					_ => false
				};
      }

			dbContext.SaveChanges ();
     }

    private static bool checkLocalLocked (Conversion conv, Action<IConversion> callback, string downloadDirectory) =>
      checkFile (conv, R.EncryptedFileExt, callback, downloadDirectory, 
				EConversionState.remote, ECheckFile.deleteIfMissing | ECheckFile.relocatable);
		

    private static bool checkLocalUnlocked (Conversion conv, Action<IConversion> callback, string downloadDirectory) {
      return checkLocal (conv, callback, downloadDirectory);
    }

    private static bool checkLocal (
			Conversion conv, 
			Action<IConversion> callback, 
			string downloadDirectory,
			EConversionState? transientfallback = null
		) {
      bool succ = checkFile (conv, R.DecryptedFileExt, callback, downloadDirectory,
        EConversionState.local_locked, ECheckFile.relocatable, transientfallback);
      if (!succ)
        succ = checkFile (conv, R.EncryptedFileExt, callback, downloadDirectory,
          EConversionState.remote, ECheckFile.deleteIfMissing | ECheckFile.relocatable, transientfallback);
      return succ;
    }

    private static bool checkExported (
			Conversion conv, Action<IConversion> callback, 
			string downloadDirectory, string exportDirectory
		) { 
			bool succ = checkFile (conv, R.ExportedFileExt, callback, exportDirectory, 
				EConversionState.local_unlocked, ECheckFile.none, EConversionState.converted_unknown);
			if (!succ)
				succ = checkLocal (conv, callback, downloadDirectory, EConversionState.converted_unknown);
			return succ;
		}

		static readonly IEnumerable<string> __extensions = new string[] { ".m3u", ".mp3", ".m4a", ".m4b" }; 
    
		private static bool checkConverted (Conversion conv, Action<IConversion> callback, string downloadDirectory) {
      bool succ = checkConvertedFiles (conv, callback);
			if (!succ)
				succ = checkLocal (conv, callback, downloadDirectory, EConversionState.converted_unknown);
			return succ;
    }

    private static bool checkConvertedFiles (Conversion conv, Action<IConversion> callback) {
      string dir = conv.DestDirectory.AsUncIfLong ();
      bool exists = false;
      if (Directory.Exists (dir)) {
        string[] files = Directory.GetFiles (dir);
        exists = files
          .Select (f => Path.GetExtension (f).ToLower ())
          .Where (e => __extensions.Contains (e))
          .Any ();
      }

      if (exists)
        return true;
      else {
        conv.State = EConversionState.converted_unknown;
        callback?.Invoke (conv);
        return false;
      }
    }

    private static bool checkFile ( 
			Conversion conv, 
			string ext,
			Action<IConversion> callback, 
			string downloadDirectory, 
			EConversionState fallback,
			ECheckFile flags,
			EConversionState? transientfallback = null
		) {

			if (flags.HasFlag (ECheckFile.relocatable)) {
				if (downloadDirectory is null)
					return false;
				string path = (conv.DownloadFileName + ext).AsUncIfLong ();
				if (File.Exists (path))
					return true;

				if (conv.DownloadFileName is not null) {
					string filename = Path.GetFileNameWithoutExtension (conv.DownloadFileName);
					string pathStub = Path.Combine (downloadDirectory, filename);
					path = (pathStub + ext).AsUncIfLong ();

					if (File.Exists (path)) {
						conv.DownloadFileName = pathStub;
						return true;
					}
				}
			} else {
				string filename = Path.GetFileNameWithoutExtension (conv.DownloadFileName);
				string pathStub = Path.Combine (downloadDirectory, filename);
				string path = (pathStub + ext).AsUncIfLong ();
				if (File.Exists (path))
					return true;
      }

      if (flags.HasFlag (ECheckFile.deleteIfMissing))
        conv.DownloadFileName = null;

			if (transientfallback.HasValue) {
				var tmp = conv.Copy (); 
				tmp.State = transientfallback.Value;
				callback?.Invoke (tmp);
			}

			updateState (conv, fallback);
			if (!transientfallback.HasValue)
				callback?.Invoke (conv);

      return false;
    }

    private static void updateState (Conversion conversion, EConversionState state, Conversion original = null) {
      conversion.State = state;
			conversion.LastUpdate = DateTime.UtcNow;
			if (original is not null) {
				original.State = conversion.State;
				original.LastUpdate = conversion.LastUpdate;
				original.PersistState = conversion.State;
      }
    }

    private static void setDownloadFilenameAndCodec (adb.json.ContentLicense license, Conversion conversion) {
      var product = conversion.BookCommon;
      // download destination
      string dir = conversion.DownloadFileName;

      var sb = new StringBuilder ();

			// title plus asin plus codec.aaxc.m4b 
			string title = product.Title.Prune ();
			title = title.Substring (0, Math.Min(20, title.Length));
      sb.Append (title);

      string asin = product.Asin;
      sb.Append ($"_{asin}_LC");

      string format = license.content_metadata?.content_reference?.content_format?.ToLower();
      bool succ = Enum.TryParse<ECodec> (format, out ECodec codec);
			if (succ) {
				product.FileCodec = codec;
				AudioQuality aq = codec.ToQuality ();
				if (aq is not null) {
					product.BitRate = aq.BitRate;
					product.SampleRate = aq.SampleRate;
					if (aq.BitRate.HasValue)
						sb.Append ($"_{aq.BitRate.Value}");
					if (aq.SampleRate.HasValue)
						sb.Append ($"_{aq.SampleRate.Value}");
				}
			}

			string filename = sb.ToString ();// + ".aaxc.m4b";
      string path = Path.Combine (dir, filename);
      conversion.DownloadFileName = path;
    }

		private static void addChapters (BookDbContext dbContext, adb.json.ContentLicense license, Conversion conversion) {

			var source = license?.content_metadata?.chapter_info; 
			if (source is null)
				return;
      
			var product = conversion.BookCommon;

			ChapterInfo chapterInfo = new ChapterInfo ();
			dbContext.ChapterInfos.Add (chapterInfo);
			if (product is Book book) {
				dbContext.Entry (book).Reference (b => b.ChapterInfo).Load ();
				if (book.ChapterInfo is not null)
					dbContext.Remove (book.ChapterInfo);
				book.ChapterInfo = chapterInfo;
			} else if (product is Component comp) {
				dbContext.Entry (comp).Reference (b => b.ChapterInfo).Load ();
				if (comp.ChapterInfo is not null)
					dbContext.Remove (comp.ChapterInfo);
				comp.ChapterInfo = chapterInfo;
			}

			chapterInfo.BrandIntroDurationMs = source.brandIntroDurationMs;
			chapterInfo.BrandOutroDurationMs = source.brandOutroDurationMs;
			chapterInfo.IsAccurate = source.is_accurate;
			chapterInfo.RuntimeLengthMs = source.runtime_length_ms;

			if (source.chapters.IsNullOrEmpty ())
				return;

			foreach (var ch in source.chapters) {
				Chapter chapter = new Chapter ();
				dbContext.Chapters.Add (chapter);
				chapterInfo.Chapters.Add (chapter);

				chapter.LengthMs = ch.length_ms;
				chapter.StartOffsetMs = ch.start_offset_ms;
				chapter.Title = ch.title;
      }
		}

    private void addBooks (List<adb.json.Product> libProducts, ProfileId profileId) {
			lock (_bookCache)
				_bookCache.Remove (profileId);
			int page = 0;
			int remaining = libProducts.Count;
			while (remaining > 0) {
				int count = Math.Min (remaining, PAGE_SIZE);
				int start = page * PAGE_SIZE;
				page++;
				remaining -= count;
				var subrange = libProducts.GetRange (start, count);
				addPageBooks (subrange, profileId);
			}
		}

		private DateTime sinceLatestPurchaseDate () {
			DateTime dt = new DateTime (1970, 1, 1);
			using var dbContext = new BookDbContextLazyLoad (_dbDir);

			var latest = dbContext.Books
					.Where (b => b.PurchaseDate.HasValue)
					.Select (b => b.PurchaseDate.Value)
					.OrderBy (b => b)
					.LastOrDefault ();
				if (latest != default)
					dt = latest + TimeSpan.FromMilliseconds (1);
			
			return dt;
		}

		private void cleanupDuplicateAuthors () {
			using var dbContext = new BookDbContextLazyLoad (_dbDir);

			var authors = dbContext.Authors;

			var duplicates = authors
				.ToList ()
				.GroupBy (x => x.Name)
				.Where (g => g.Count () > 1)
				.ToList ();

			const int PseudoKeyLength = 7;
			foreach (var d in duplicates) {
				var asinAuthor = d.FirstOrDefault (d => d.Asin.Length > PseudoKeyLength);
				if (asinAuthor is null)
					continue;
				foreach (var author in d) {
					if (author == asinAuthor)
						continue;

					foreach (var book in author.Books) {
						book.Authors.Remove (author);
						book.Authors.Add (asinAuthor);
					}
					authors.Remove (author);

				}
			}

			dbContext.SaveChanges ();
		}

		private void addPageBooks (IEnumerable<adb.json.Product> products, ProfileId profileId) {
			using var _ = new LogGuard (3, this, () => $"#items={products.Count()}");

			using var dbContext = new BookDbContextLazyLoad (_dbDir);

			var bookAsins = dbContext.Books.Select (b => b.Asin).ToList ();
			var conversions = dbContext.Conversions.ToList ();
			var components = dbContext.Components.ToList ();
			var series = dbContext.Series.ToList ();
			var seriesBooks = dbContext.SeriesBooks.ToList ();
			var authors = dbContext.Authors.ToList ();
			var narrators = dbContext.Narrators.ToList ();
			var genres = dbContext.Genres.ToList ();
			var ladders = dbContext.Ladders.ToList ();
			var rungs = dbContext.Rungs.ToList ();
			var codecs = dbContext.Codecs.ToList ();

			foreach (var product in products) {
				if (bookAsins.Contains (product.asin))
					continue;

				Book book = addBook (dbContext, product);
				

				addComponents (book, components, product.relationships);
				
				addConversions (book, conversions, profileId);
				
				addSeries (book, series, seriesBooks, product.relationships);

				addPersons (dbContext, book, authors, product.authors, b => b.Authors);
				addPersons (dbContext, book, narrators, product.narrators, b => b.Narrators);

				addGenres (book, genres, ladders, rungs, product.category_ladders);

				addCodecs (book, codecs, product.available_codecs);

			}

			dbContext.SaveChanges ();

		}

		private static Book addBook (BookDbContextLazyLoad dbContext, adb.json.Product product) {
			Book book = new Book {
				Asin = product.asin,
				Title = product.title,
				Subtitle = product.subtitle,
				PublisherName = product.publisher_name,
				PublisherSummary = product.publisher_summary,
				MerchandisingSummary = product.merchandising_summary,
				AverageRating = product.rating?.overall_distribution?.average_rating,
				RunTimeLengthSeconds = product.runtime_length_min.HasValue ? product.runtime_length_min.Value * 60 : null,
				AdultProduct = product.is_adult_product,
				PurchaseDate = product.purchase_date,
				ReleaseDate = product.release_date ?? product.issue_date,
				Language = product.language,
				CoverImageUrl = product.product_images?._500,
				Sku = product.sku,
				SkuLite = product.sku_lite
			};

			bool succ = Enum.TryParse<EDeliveryType> (product.content_delivery_type, out var deltype);
			if (succ)
				book.DeliveryType = deltype;

			if (!product.format_type.IsNullOrEmpty ())
				book.Unabridged = product.format_type == "unabridged";

			dbContext.Books.Add (book);
			return book;
		}

		private static void addComponents (Book book, ICollection<Component> components, IEnumerable<adb.json.Relationship> itmRelations) {
      var relations = itmRelations?
				.Where (r => r.relationship_to_product == "child" && r.relationship_type == "component")
				.ToList ();

			if (relations.IsNullOrEmpty ())
				return;

      foreach (var rel in relations) {
        int.TryParse (rel.sort, out int partNum);
				var component = new Component {
					Asin = rel.asin,
					Title = rel.title,
					Sku = rel.sku,
					SkuLite = rel.sku_lite,
					PartNumber = partNum
				};

				components.Add (component);
				book.Components.Add (component);
			}
    }

    const string REGEX_SERIES = @"((\d*)(\.(\d+))?)";
		static readonly Regex _regexSeries = new Regex (REGEX_SERIES, RegexOptions.Compiled);

		private static void addSeries (Book book, ICollection<Series> series, ICollection<SeriesBook> seriesBooks, IEnumerable<adb.json.Relationship> itmRelations) {
			if (itmRelations is null)
				return;

			var itmSeries = itmRelations.Where (r => r.relationship_to_product == "parent" && r.relationship_type == "series").ToList ();

			foreach (var itmSerie in itmSeries) {
				var serie = series.FirstOrDefault (s => s.Asin == itmSerie.asin);
				if (serie is null) {
					serie = new Series {
						Asin = itmSerie.asin,
						Title = itmSerie.title,
						Sku = itmSerie.sku,
						SkuLite = itmSerie.sku_lite
					};
					series.Add (serie);
				}

				Match match = _regexSeries.Match (itmSerie.sequence);
				if (!match.Success)
					return;

				int n = match.Groups.Count;
				if (n < 3)
					return;

				var seriesBook = new SeriesBook {
					Book = book,
					Series = serie
				};

				string major = match.Groups[2].Value;
				seriesBook.BookNumber = int.Parse (major);

				if (n >= 4) {
					string minor = match.Groups[4].Value;
					bool succ = int.TryParse (minor, out int subnum);
					if (succ)
						seriesBook.SubNumber = int.Parse (minor);
				}

				seriesBooks.Add (seriesBook);
				book.Series.Add (seriesBook);
			}
    }

		private static void addPersons<TPerson> (
			BookDbContextLazyLoad dbContext,
			Book book,
			ICollection<TPerson> persons,
			IEnumerable<adb.json.IPerson> itmPersons,
			Func<Book, ICollection<TPerson>> getBookPersons
		)
			where TPerson : class, IPerson, new() 
	  {
			if (itmPersons is null)
				return;

			foreach (var itmPerson in itmPersons) {
				TPerson person = null;
				if (itmPerson.asin is null) {
					person = persons.FirstOrDefault (a => a.Name == itmPerson.name);
					if (person is null)
						itmPerson.asin = dbContext.GetNextPseudoAsin (typeof (TPerson));
				}
				if (person is null)
					person = persons.FirstOrDefault (a => a.Asin == itmPerson.asin);

				if (person is null) {
					person = new TPerson {
						Asin = itmPerson.asin,
						Name = itmPerson.name
					};

					persons.Add (person);

				}
				person.Books.Add (book);
				getBookPersons (book).Add (person);
			}
		}


		private static void addGenres (Book book, ICollection<Genre> genres, ICollection<Ladder> ladders, ICollection<Rung> rungs, IEnumerable<adb.json.Category> itmCategories) {
			if (itmCategories is null)
				return;

			var categories = itmCategories.Where (c => c.root == "Genres").ToList ();

			foreach (var category in categories) {
				var ladder = new Ladder ();

				for (int i = 0; i < category.ladder.Length; i++) {
					var itmLadder = category.ladder[i];
					int idx = i + 1;
					bool succ = long.TryParse (itmLadder.id, out long id);
					if (!succ)
						continue;

					var genre = genres.FirstOrDefault (g => g.ExternalId == id);
					if (genre is null) {
						genre = new Genre {
							ExternalId = id,
							Name = itmLadder.name
						};
						genres.Add (genre);
          }

					book.Genres.Add (genre);

					var rung = rungs.FirstOrDefault (r => r.OrderIdx == idx && r.Genre == genre);
					if (rung is null) {
						rung = new Rung {
							OrderIdx = idx,
							Genre = genre
						};
						rungs.Add (rung);
					}

					ladder.Rungs.Add (rung);

        }

				var existingLadder = ladders.FirstOrDefault (l => equals (l, ladder));

        if (existingLadder is null)
          ladders.Add (ladder);
        else
          ladder = existingLadder;

        book.Ladders.Add (ladder);

			}
    
			// local function
			static bool equals (Ladder oldLadder, Ladder newLadder) {
				if (newLadder.Rungs.Count != oldLadder.Rungs.Count)
					return false;
				var rungs = oldLadder.Rungs.OrderBy (r => r.OrderIdx);
				var iter1 = newLadder.Rungs.GetEnumerator ();
				var iter2 = rungs.GetEnumerator ();
				while (iter1.MoveNext ()) {
					iter2.MoveNext ();
					var r1 = iter1.Current;
					var r2 = iter2.Current;
					if (r1.Genre != r2.Genre)
						return false;
				}
				return true;
			}
		}


    private static void addCodecs (Book book, ICollection<Codec> codecList, IEnumerable<adb.json.Codec> itmCodecs) {
      if (itmCodecs is null)
        return;

      foreach (var itmCodec in itmCodecs) {
        bool succ = Enum.TryParse<ECodec> (itmCodec.name, out var codecName);
        if (!succ)
          continue;

        var codec = codecList.FirstOrDefault (c => c.Name == codecName);
        if (codec is null) {
          codec = new Codec {
            Name = codecName
          };

          codecList.Add (codec);
        }

        book.Codecs.Add (codec);
      }
    }

		private static void addConversions (Book book, ICollection<Conversion> conversions, ProfileId profileId) {
			// default
			{
				var conversion = new Conversion {
					AccountId = profileId.AccountId,
					Region = profileId.Region
				};
				updateState (conversion, EConversionState.remote);
				book.Conversion = conversion;
				conversions.Add (conversion);
			}

			// components
			foreach (var component in book.Components) {
				if (component.Conversion is not null)
					continue;

				var conversion = new Conversion {
					State = EConversionState.remote,
					AccountId = profileId.AccountId,
					Region = profileId.Region
				};
				component.Conversion = conversion;
				conversions.Add (conversion);
      }

		}
  }

}