namespace OpenAPI.GameEngine.Games.Teams
{
    public enum TeamFillMode
    {
        /// <summary>
        ///     Fill a team completely before going on to the next.
        /// </summary>
        Fill,
        /// <summary>
        ///     Fill all teams evenly
        /// </summary>
        Spread,
        /// <summary>
        /// Fills every team up to its minimum member count, then tries filling them evenly after
        /// </summary>
        FillMinSpread
    }
}