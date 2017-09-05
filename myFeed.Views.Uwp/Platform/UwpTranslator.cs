using myFeed.Services.Abstractions;

namespace myFeed.Views.Uwp.Platform {
    public class UwpTranslator : ITranslationsService {
        public string Resolve(string name) => "I am Uwp translator";
    }
}
