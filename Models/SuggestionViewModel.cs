using System.Collections.Generic;
using Ezequiel_Movies.Models.TmdbApi;

namespace Ezequiel_Movies.Models
{
    // This class will hold one category of suggestions,
    // like "From your most watched director"
    public class SuggestionCategory
    {
        public string? Title { get; set; }
        public List<TmdbMovieBrief> Movies { get; set; } = new();
    }

    // This is the main model that our Suggest.cshtml page will use
    public class SuggestionViewModel
    {
        public List<SuggestionCategory> Categories { get; set; } = new();
    }
}