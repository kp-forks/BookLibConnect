﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using core.audiamus.booksdb;

namespace core.audiamus.booksdb.Migrations
{
    [DbContext(typeof(BookDbContext))]
    partial class BookDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("AuthorBook", b =>
                {
                    b.Property<int>("AuthorsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BooksId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AuthorsId", "BooksId");

                    b.HasIndex("BooksId");

                    b.ToTable("AuthorBook");
                });

            modelBuilder.Entity("BookCodec", b =>
                {
                    b.Property<int>("BooksId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CodecsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BooksId", "CodecsId");

                    b.HasIndex("CodecsId");

                    b.ToTable("BookCodec");
                });

            modelBuilder.Entity("BookGenre", b =>
                {
                    b.Property<int>("BooksId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GenresId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BooksId", "GenresId");

                    b.HasIndex("GenresId");

                    b.ToTable("BookGenre");
                });

            modelBuilder.Entity("BookLadder", b =>
                {
                    b.Property<int>("BooksId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LaddersId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BooksId", "LaddersId");

                    b.HasIndex("LaddersId");

                    b.ToTable("BookLadder");
                });

            modelBuilder.Entity("BookNarrator", b =>
                {
                    b.Property<int>("BooksId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NarratorsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BooksId", "NarratorsId");

                    b.HasIndex("NarratorsId");

                    b.ToTable("BookNarrator");
                });

            modelBuilder.Entity("LadderRung", b =>
                {
                    b.Property<int>("LaddersId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RungsOrderIdx")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RungsGenreId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LaddersId", "RungsOrderIdx", "RungsGenreId");

                    b.HasIndex("RungsOrderIdx", "RungsGenreId");

                    b.ToTable("LadderRung");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Alias")
                        .HasColumnType("TEXT");

                    b.Property<string>("AudibleId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("AdultProduct")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<float?>("AverageRating")
                        .HasColumnType("REAL");

                    b.Property<int?>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CoverImageFile")
                        .HasColumnType("TEXT");

                    b.Property<string>("CoverImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DeliveryType")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FileCodec")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("FileSizeBytes")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .HasColumnType("TEXT");

                    b.Property<string>("LicenseIv")
                        .HasColumnType("TEXT");

                    b.Property<string>("LicenseKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("MerchandisingSummary")
                        .HasColumnType("TEXT");

                    b.Property<string>("PublisherName")
                        .HasColumnType("TEXT");

                    b.Property<string>("PublisherSummary")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PurchaseDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Removed")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RunTimeLengthSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SampleRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Sku")
                        .HasColumnType("TEXT");

                    b.Property<string>("SkuLite")
                        .HasColumnType("TEXT");

                    b.Property<string>("Subtitle")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("Unabridged")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.HasIndex("PurchaseDate");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Chapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LengthMs")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StartOffsetMs")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChapterInfoId");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("core.audiamus.booksdb.ChapterInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BookId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BrandIntroDurationMs")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BrandOutroDurationMs")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ComponentId")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("IsAccurate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RuntimeLengthMs")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BookId")
                        .IsUnique();

                    b.HasIndex("ComponentId")
                        .IsUnique();

                    b.ToTable("ChapterInfos");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Codec", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Name")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Codecs");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Component", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<int?>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FileCodec")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("FileSizeBytes")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LicenseIv")
                        .HasColumnType("TEXT");

                    b.Property<string>("LicenseKey")
                        .HasColumnType("TEXT");

                    b.Property<int>("PartNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("RunTimeLengthSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SampleRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Sku")
                        .HasColumnType("TEXT");

                    b.Property<string>("SkuLite")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.HasIndex("BookId");

                    b.ToTable("Components");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Conversion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AveTrackLengthMinutes")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BookId")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("ChapterMarkAdjusting")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ComponentId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConvFormat")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ConvMode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DestDirectory")
                        .HasColumnType("TEXT");

                    b.Property<string>("DownloadFileName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Mp4AAudio")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("NamedChapters")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("PreferEmbChapMarks")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ReducedBitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Region")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ShortChapDurSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("VariableBitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("VeryShortChapDurSeconds")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BookId")
                        .IsUnique();

                    b.HasIndex("ComponentId")
                        .IsUnique();

                    b.ToTable("Conversions");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ExternalId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ExternalId")
                        .IsUnique();

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Ladder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Ladders");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Narrator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.ToTable("Narrators");
                });

            modelBuilder.Entity("core.audiamus.booksdb.PseudoAsin", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LatestId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PseudoAsins");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Rung", b =>
                {
                    b.Property<int>("OrderIdx")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GenreId")
                        .HasColumnType("INTEGER");

                    b.HasKey("OrderIdx", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("Rungs");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Series", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Sku")
                        .HasColumnType("TEXT");

                    b.Property<string>("SkuLite")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.ToTable("Series");
                });

            modelBuilder.Entity("core.audiamus.booksdb.SeriesBook", b =>
                {
                    b.Property<int>("SeriesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Sequence")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Sort")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SubNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("SeriesId", "BookId");

                    b.HasIndex("BookId");

                    b.ToTable("SeriesBooks");
                });

            modelBuilder.Entity("AuthorBook", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Author", null)
                        .WithMany()
                        .HasForeignKey("AuthorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BookCodec", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Codec", null)
                        .WithMany()
                        .HasForeignKey("CodecsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BookGenre", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Genre", null)
                        .WithMany()
                        .HasForeignKey("GenresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BookLadder", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Ladder", null)
                        .WithMany()
                        .HasForeignKey("LaddersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BookNarrator", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Narrator", null)
                        .WithMany()
                        .HasForeignKey("NarratorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LadderRung", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Ladder", null)
                        .WithMany()
                        .HasForeignKey("LaddersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Rung", null)
                        .WithMany()
                        .HasForeignKey("RungsOrderIdx", "RungsGenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("core.audiamus.booksdb.Chapter", b =>
                {
                    b.HasOne("core.audiamus.booksdb.ChapterInfo", "ChapterInfo")
                        .WithMany("Chapters")
                        .HasForeignKey("ChapterInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ChapterInfo");
                });

            modelBuilder.Entity("core.audiamus.booksdb.ChapterInfo", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", "Book")
                        .WithOne("ChapterInfo")
                        .HasForeignKey("core.audiamus.booksdb.ChapterInfo", "BookId");

                    b.HasOne("core.audiamus.booksdb.Component", "Component")
                        .WithOne("ChapterInfo")
                        .HasForeignKey("core.audiamus.booksdb.ChapterInfo", "ComponentId");

                    b.Navigation("Book");

                    b.Navigation("Component");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Component", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", "Book")
                        .WithMany("Components")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Conversion", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", "Book")
                        .WithOne("Conversion")
                        .HasForeignKey("core.audiamus.booksdb.Conversion", "BookId");

                    b.HasOne("core.audiamus.booksdb.Component", "Component")
                        .WithOne("Conversion")
                        .HasForeignKey("core.audiamus.booksdb.Conversion", "ComponentId");

                    b.Navigation("Book");

                    b.Navigation("Component");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Rung", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Genre", "Genre")
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("core.audiamus.booksdb.SeriesBook", b =>
                {
                    b.HasOne("core.audiamus.booksdb.Book", "Book")
                        .WithMany("Series")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("core.audiamus.booksdb.Series", "Series")
                        .WithMany("Books")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Book", b =>
                {
                    b.Navigation("ChapterInfo");

                    b.Navigation("Components");

                    b.Navigation("Conversion");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("core.audiamus.booksdb.ChapterInfo", b =>
                {
                    b.Navigation("Chapters");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Component", b =>
                {
                    b.Navigation("ChapterInfo");

                    b.Navigation("Conversion");
                });

            modelBuilder.Entity("core.audiamus.booksdb.Series", b =>
                {
                    b.Navigation("Books");
                });
#pragma warning restore 612, 618
        }
    }
}
