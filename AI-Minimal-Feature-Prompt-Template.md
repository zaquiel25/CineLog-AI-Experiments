# Minimal Feature Prompt Template for AI Assistants

## Title
Display [Field(s)] on [Page/View] with Fallback, No Migrations

## Prompt
> On the [Page/View], display the [Field(s)] (e.g., Director, Year) for each [item/card/row].  
> - If the data is available in the user’s [local table/model], use it.  
> - If not, fetch it live from [external API/service] for display only.  
> - Do NOT add or change any database fields, models, or migrations—this is a display-only feature.  
> - Keep the code DRY, minimal, and maintainable.  
> - If data is missing, show a clear fallback (e.g., “Unknown” or “Not available”).  
> - Do not overengineer: prefer a simple per-item lookup and fallback, not a JOIN or new helpers unless truly needed.

## Example
> - Use the user’s Movies table if available.  
> - Otherwise, fetch from TMDB.  
> - No migrations or model changes.  
> - Minimal code, DRY, and maintainable.

### Example 2: Show Poster on Blacklist Page
> On the Blacklist page, display the movie poster for each blacklisted movie.  
> - Use the stored PosterUrl if available.  
> - If not, fetch the poster path from TMDB for display only.  
> - Do not add or change any database fields or run migrations.  
> - Keep the code minimal and DRY.

### Example 3: Display Cast on Movie Details
> On the Movie Details page, show the top 3 cast members for each movie.  
> - If the cast is available in the local model, use it.  
> - Otherwise, fetch the cast from TMDB for display only.  
> - No migrations or model changes.  
> - Prefer a simple fallback and minimal code.

---

## Tips for Use
- Always specify “no migrations/model changes” for display-only features.
- State where to get the data and what fallback to use.
- Ask for the “minimal code” or “simplest approach.”
- If you see a complex solution, ask: “Can you show me a simpler, display-only version?”
