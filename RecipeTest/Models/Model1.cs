using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;

namespace RecipeTest.Models
{
    public class Model1 : DbContext
    {
        // 您的內容已設定為使用應用程式組態檔 (App.config 或 Web.config)
        // 中的 'Model1' 連接字串。根據預設，這個連接字串的目標是
        // 您的 LocalDb 執行個體上的 'RecipeTest.Models.Model1' 資料庫。
        // 
        // 如果您的目標是其他資料庫和 (或) 提供者，請修改
        // 應用程式組態檔中的 'Model1' 連接字串。
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Recipe> Recipe { get; set; }
        public virtual DbSet<RecipePhotos> RecipePhotos { get; set; }
        public virtual DbSet<Ingredients> Ingredients { get; set; }
        public virtual DbSet<StepPhotos> StepPhotos { get; set; }
        public virtual DbSet<SubSteps> SubSteps { get; set; }
        public virtual DbSet<User> User {  get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<IngredientsAmounts>()
            //    .HasKey(ia => new { ia.RecipeId, ia.IngredientId }); // 設定複合主鍵
            modelBuilder.Entity<User>().HasIndex(u => u.AccountEmail).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.AccountName).IsUnique();
            modelBuilder.Entity<Recipe>()
                .Property(r => r.CookingTime)
                .HasPrecision(10, 2);
            modelBuilder.Entity<Recipe>()
                .Property(r => r.Portion)
                .HasPrecision(10, 2);  // 確保 Portion 是 decimal(10,2)

            modelBuilder.Entity<Recipe>()
                .Property(r => r.Rating)
                .HasPrecision(10, 2);  // 確保 Rating 是 decimal(10,2)
            base.OnModelCreating(modelBuilder);
        }


        // 針對您要包含在模型中的每種實體類型新增 DbSet。如需有關設定和使用
        // Code First 模型的詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}