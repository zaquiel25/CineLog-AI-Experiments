-- Production Performance Indexes for CineLog Database
-- Generated: 2025-01-30
-- Purpose: Additional indexes to optimize production query performance

-- =============================================================================
-- MOVIES TABLE PERFORMANCE INDEXES
-- =============================================================================

-- Index for DateWatched queries (suggestion system frequently queries recent movies)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_DateWatched] 
ON [Movies] ([UserId] ASC, [DateWatched] DESC)
WHERE [DateWatched] IS NOT NULL;

-- Index for Director-based queries (director suggestions)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_Director] 
ON [Movies] ([UserId] ASC, [Director] ASC)
WHERE [Director] IS NOT NULL;

-- Index for Genre-based queries (genre suggestions)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_Genres] 
ON [Movies] ([UserId] ASC, [Genres] ASC)
WHERE [Genres] IS NOT NULL;

-- Index for Rating-based queries (high-rated movie filtering)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_UserRating] 
ON [Movies] ([UserId] ASC, [UserRating] DESC)
WHERE [UserRating] IS NOT NULL;

-- Index for TMDB ID lookups (preventing duplicate movie additions)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_TmdbId] 
ON [Movies] ([UserId] ASC, [TmdbId] ASC)
WHERE [TmdbId] IS NOT NULL;

-- Index for year-based queries (decade suggestions)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_ReleasedYear] 
ON [Movies] ([UserId] ASC, [ReleasedYear] ASC)
WHERE [ReleasedYear] IS NOT NULL;

-- Composite index for sorting by DateCreated (fallback for DateWatched)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_DateCreated] 
ON [Movies] ([UserId] ASC, [DateCreated] DESC);

-- Index for full-text search queries (List page search)
CREATE NONCLUSTERED INDEX [IX_Movies_UserId_Title] 
ON [Movies] ([UserId] ASC, [Title] ASC);

-- =============================================================================
-- WISHLIST ITEMS PERFORMANCE INDEXES
-- =============================================================================

-- Index for TMDB ID lookups (checking if movie exists in wishlist)
CREATE NONCLUSTERED INDEX [IX_WishlistItems_UserId_TmdbId] 
ON [WishlistItems] ([UserId] ASC, [TmdbId] ASC);

-- Index for date-based sorting (Wishlist page)
CREATE NONCLUSTERED INDEX [IX_WishlistItems_UserId_DateAdded] 
ON [WishlistItems] ([UserId] ASC, [DateAdded] DESC);

-- =============================================================================
-- BLACKLISTED MOVIES PERFORMANCE INDEXES
-- =============================================================================
-- Note: IX_BlacklistedMovies_UserId_TmdbId already exists from migration

-- Index for date-based sorting (Blacklist page)
CREATE NONCLUSTERED INDEX [IX_BlacklistedMovies_UserId_BlacklistedDate] 
ON [BlacklistedMovies] ([UserId] ASC, [BlacklistedDate] DESC);

-- Index for title-based searching (Blacklist page search)
CREATE NONCLUSTERED INDEX [IX_BlacklistedMovies_UserId_Title] 
ON [BlacklistedMovies] ([UserId] ASC, [Title] ASC);

-- =============================================================================
-- IDENTITY TABLES PERFORMANCE OPTIMIZATIONS
-- =============================================================================
-- Note: Identity tables already have proper indexes from ASP.NET Core Identity

-- =============================================================================
-- PERFORMANCE MONITORING QUERIES
-- =============================================================================

-- Query to check index usage statistics
-- SELECT 
--     i.name AS IndexName,
--     t.name AS TableName,
--     s.user_seeks,
--     s.user_scans,
--     s.user_lookups,
--     s.user_updates
-- FROM sys.dm_db_index_usage_stats s
-- INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
-- INNER JOIN sys.tables t ON i.object_id = t.object_id
-- WHERE s.database_id = DB_ID('Ezequiel_Movies')
-- ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;

-- Query to identify missing indexes
-- SELECT 
--     migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
--     'CREATE NONCLUSTERED INDEX [IX_' + OBJECT_NAME(mid.object_id) + '_' + 
--     REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns,''), ', ', '_'), '[', ''), ']', '') +
--     CASE WHEN mid.inequality_columns IS NOT NULL THEN '_' + 
--     REPLACE(REPLACE(REPLACE(mid.inequality_columns, ', ', '_'), '[', ''), ']', '') ELSE '' END + ']' +
--     ' ON [' + OBJECT_NAME(mid.object_id) + '] (' + ISNULL(mid.equality_columns,'') +
--     CASE WHEN mid.inequality_columns IS NOT NULL AND mid.equality_columns IS NOT NULL THEN ',' ELSE '' END +
--     ISNULL(mid.inequality_columns, '') + ')' +
--     CASE WHEN mid.included_columns IS NOT NULL THEN ' INCLUDE (' + mid.included_columns + ')' ELSE '' END AS create_index_statement,
--     migs.*,
--     mid.database_id,
--     mid.object_id
-- FROM sys.dm_db_missing_index_groups mig
-- INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
-- INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
-- WHERE migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) > 10
-- AND mid.database_id = DB_ID('Ezequiel_Movies')
-- ORDER BY migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) DESC;