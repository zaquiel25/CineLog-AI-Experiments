using System;
using Ezequiel_Movies.Models;
using Microsoft.AspNetCore.Identity;

namespace Ezequiel_Movies1.Models.Entities
{
    public class Movies
    {
        public Movies()
        {
            Title = string.Empty;
            UserId = string.Empty;
            DateCreated = DateTime.UtcNow;
        }
/// <summary>
/// Fecha de creación del registro de película en CineLog.
/// </summary>
/// <remarks>
/// Se utiliza como fallback para ordenamiento y sugerencias cuando DateWatched es null.
/// Decisión: DateCreated es inmutable y se setea automáticamente al crear el registro.
/// Trade-off: No se expone al usuario ni se edita nunca, solo para lógica interna y robustez.
/// </remarks>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Director { get; set; }
        public int? ReleasedYear { get; set; }
        public DateTime? DateWatched { get; set; }
        public WatchedLocationType? WatchedLocation { get; set; }
        public bool Subscribed { get; set; }
        public string? PosterPath { get; set; }
        public string? Overview { get; set; }
        public bool IsRewatch { get; set; }
        public decimal? UserRating { get; set; }
        public int? TmdbId { get; set; }
        public string? Genres { get; set; }
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}

