using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RecipeTest.Models;

public class Favorites
{
    [Key]
    [Column(Order = 0)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 1)]
    public int RecipeId { get; set; }

    [Column(TypeName = "DATETIME")]
    [Display(Name = "創建時間")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column(TypeName = "DATETIME")]
    [Display(Name = "更新時間")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public virtual Users User { get; set; }

    [JsonIgnore]
    public virtual Recipes Recipe { get; set; }
}
