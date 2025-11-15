namespace Fall2025_Project3_kmkummer.ViewModels;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
public class MovieActorEditViewModel
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public int ActorId { get; set; }

    public List<SelectListItem> MovieOptions { get; set; } = new();
    public List<SelectListItem> ActorOptions { get; set; } = new ();
}