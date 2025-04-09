using System;
using System.Data.Entity;

namespace RecipeTest.Models
{
    public class RecipeModel : DbContext
    {
        public RecipeModel()
            : base("name=RecipeModel")
        {
        }

        public virtual DbSet<Recipes> Recipes { get; set; }
        public virtual DbSet<RecipePhotos> RecipePhotos { get; set; }
        public virtual DbSet<Ingredients> Ingredients { get; set; }
        public virtual DbSet<StepPhotos> StepPhotos { get; set; }
        public virtual DbSet<SubSteps> SubSteps { get; set; }
        public virtual DbSet<Steps> Steps { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Follows> Follows { get; set; }
        public virtual DbSet<Favorites> Favorites { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<RecipeTags> RecipeTags { get; set; }

        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Ratings> Ratings { get; set; }
        public virtual DbSet<AdTags> AdTags { get; set; }
        public virtual DbSet<AdImgs> AdImgs { get; set; }
        public virtual DbSet<Advertisement> Advertisements { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<RecipeTags>()
    .HasKey(rt => new { rt.RecipeId, rt.TagId });

            modelBuilder.Entity<RecipeTags>()
                .HasRequired(rt => rt.Recipes)
                .WithMany(r => r.RecipeTags)
                .HasForeignKey(rt => rt.RecipeId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RecipeTags>()
                .HasRequired(rt => rt.Tags)
                .WithMany(t => t.RecipeTags)
                .HasForeignKey(rt => rt.TagId)
                .WillCascadeOnDelete(false);

            // 🔹 User → Recipes：改為 false，避免循環錯誤
            //記得之後再程式碼記得刪除
            modelBuilder.Entity<Recipes>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            // 🔹 User → Favorites：刪除 User 時，刪除他的收藏
            modelBuilder.Entity<Favorites>()
                .HasRequired(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(true);

            // 🔹 User → Follows（我追蹤了誰）
            modelBuilder.Entity<Follows>()
                .HasRequired(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(true);

            // 🔹 User → Follows（誰追蹤我）
            modelBuilder.Entity<Follows>()
                .HasRequired(f => f.FollowedUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowedUserId)
                .WillCascadeOnDelete(false);

            // 🔹 Recipes → Favorites：刪除 Recipes 時，刪除收藏
            modelBuilder.Entity<Favorites>()
                .HasRequired(f => f.Recipe)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RecipeId)
                .WillCascadeOnDelete(true);

            // 🔹 設定 User 的唯一索引
            modelBuilder.Entity<Users>().HasIndex(u => u.AccountEmail).IsUnique();
            modelBuilder.Entity<Users>().HasIndex(u => u.AccountName).IsUnique();

            // 🔹 設定 Recipes 的 decimal 精度
            modelBuilder.Entity<Recipes>()
                .Property(r => r.CookingTime)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Recipes>()
                .Property(r => r.Portion)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Recipes>()
                .Property(r => r.Rating)
                .HasPrecision(10, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
