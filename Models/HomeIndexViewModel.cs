namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// View model for the home page hero.
    /// </summary>
    public class HomeIndexViewModel
    {
        /// <summary>
        /// Up to three posters for the hero: the user's latest logged movies, or TMDB trending for visitors.
        /// </summary>
        public List<HomePoster> Posters { get; set; } = new();
    }

    /// <summary>
    /// A poster shown in the home hero.
    /// </summary>
    /// <param name="Title">Movie title, used as alt text.</param>
    /// <param name="PosterPath">TMDB poster path (e.g. "/abc.jpg").</param>
    public record HomePoster(string Title, string PosterPath);
}
