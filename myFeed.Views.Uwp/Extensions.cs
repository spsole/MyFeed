namespace myFeed.Views.Uwp {
    internal static class Extensions {
        public static bool IsDefault(this object instance) {
            switch (instance) {
                case bool b: return b == false;
                case int i: return i == 0;
                default: return instance == null;
            }
        }
    }
}
