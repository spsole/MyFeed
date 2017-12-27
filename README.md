<a href="https://www.microsoft.com/en-us/store/apps/myfeed/9nblggh4nw02">
    <img src="/myFeed.png" width="300px">
</a>

<br />

Read news from websites via <b>myFeed</b>, find interesting feeds in the internet and group them by categories, save favorite articles to your device with an ability to read them offline, share interesting posts with friends. myFeed makes the process of reading RSS feeds ever more convenient.

<br />

<a href="https://www.microsoft.com/en-us/store/apps/myfeed/9nblggh4nw02">
  <img src="https://worldbeater.github.io/mockups/myFeed.png" width="680"/>
</a>

## About App

myFeed is an RSS reader for <b>Universal Windows Platform</b> (also planned support for more platforms using Xamarin and Avalonia UI frameworks). This app can collect news from websites, save them to your device and send toast notifications once new posts appear in your news feeds. Application is completely free and open-source! Built to demonstrate how modern cross-platform app architecture can look like. <a href="https://www.microsoft.com/en-us/store/apps/myfeed/9nblggh4nw02">Get myFeed from Windows Store</a>

## Application Architecture

myFeed utilises <b>MVVM</b> (Model-View-ViewModel) architecture. ViewModels provide Views abstractions with UI logic, exposing public properties and commands. Views are specific for each platform supported by myFeed, ViewModels and Models do not rely on Views. Models represent real state content and logic; this part of MVVM pattern is implemented as Services layer, which provides logic for feed fetching, serializing, caching. All modules are combined together using <a href="https://en.wikipedia.org/wiki/Inversion_of_control">Inversion of Control</a> and <a href="https://en.wikipedia.org/wiki/Dependency_inversion_principle">Dependency Inversion</a> principles: this architectural approach improves testability and extensibility of the application.

## Technologies and Tools Used

- <a href="https://docs.microsoft.com/en-us/dotnet/csharp/csharp">C Sharp</a> and <a href="https://docs.microsoft.com/en-us/dotnet/fsharp/">F Sharp</a> programming languages 
- <a href="https://docs.microsoft.com/en-us/dotnet/standard/net-standard">.NET Standard Library</a> to reuse code on various platforms 
- <a href="http://xunit.github.io/">xUnit</a> tests on <a href="https://www.microsoft.com/net/core">.NET Core</a> with <a href="https://github.com/nsubstitute/NSubstitute">NSubstitute</a>
- <a href="https://github.com/mbdavid/LiteDB">LiteDB</a> as a NoSQL database engine
- <a href="https://bitbucket.org/dadhi/dryioc/">DryIoc</a> and <a href="https://bitbucket.org/dadhi/dryioc/wiki/Extensions/MefAttributedModel">DryIoc.Attributes</a> for <a href="https://en.wikipedia.org/wiki/Dependency_injection">Dependency Injection</a>
- <a href="https://github.com/codehollow/FeedReader">CodeHollow.FeedReader</a> to parse RSS feeds
- <a href="https://www.newtonsoft.com/json">Newtonsoft.Json</a> to parse Json objects
- <a href="https://developer.microsoft.com/en-us/windows/apps">Universal Windows Platform</a>
- <a href="https://github.com/Microsoft/UWPCommunityToolkit">UWP Community Toolkit</a>
- <a href="https://github.com/Microsoft/XamlBehaviors">UWP XAML Behaviors SDK</a>
- <a href="https://code.visualstudio.com/">Visual Studio Code</a> with <a href="http://ionide.io/">Ionide plugin</a>
- <a href="https://www.jetbrains.com/rider/">JetBrains Rider</a> IDE
- <a href="https://www.visualstudio.com/ru/vs/whatsnew/">Visual Studio 2017</a> with <a href="https://www.jetbrains.com/resharper/">JetBrains ReSharper</a> plugin
