namespace myFeed.Services.Abstractions {
    /// <summary>
    /// Translations service that provides country-based phrases.
    /// </summary>
    public interface ITranslationsService {
        /// <summary>
        /// Resolves given resourse name.
        /// </summary>
        /// <param name="name">Name to resolve.</param>
        string Resolve(string name);
    }
}
