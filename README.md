# myFeed
Read news from websites via <b>myFeed</b>, find interesting feeds in the internet and group them by categories, save favorite articles to your device with an ability to read them offline, share interesting posts with friends.

<b>myFeed</b> has an intuitive interface and works fast, this makes the process of reading RSS feeds ever more convenient. 

<a href="https://www.microsoft.com/en-us/store/apps/myfeed/9nblggh4nw02">Get myFeed from Windows Store for your mobile or desktop Windows 10 device right now!</a>

<a href="https://www.microsoft.com/en-us/store/apps/myfeed/9nblggh4nw02">
  <img src="https://worldbeater.github.io/mockups/myFeed.png" width="670"/>
</a>

## About

myFeed is an RSS reader for <b>Universal Windows Platform</b> (also planned support for Android and Linux). This app can collect news from websites, save them to your device and send toast notifications for you once new posts appear in your news feeds.

Application is completely free and open-source. No ads, no subscription fees. Unlimited trial version contains all features of a full one, but if you appreciate my work and would like to support myFeed's future development, please, consider donating.

## Application architecture

myFeed utilises <b>MVVM</b> (Model-View-ViewModel) architectural pattern. <b>ViewModels</b> provide Views abstractions, exposing public properties and commands. <b>Views</b> are specific for each platform supported by myFeed (currently UWP Views only); ViewModels and Models do not rely on Views. <b>Models</b> represent real state content and logic; this part of MVVM pattern is implemented as <b>Services</b> and <b>Repositories</b> layers. There is a dependencies graph below to make all these points clear:

<img src="/DependenciesGraph.png" width="400px">

## Technologies and Tools used

- <a href="https://docs.microsoft.com/en-us/dotnet/csharp/csharp">C Sharp</a> and <a href="https://docs.microsoft.com/en-us/dotnet/fsharp/">F Sharp</a> programming languages 
- <a href="https://docs.microsoft.com/en-us/dotnet/standard/net-standard">.NET Standard Library</a> to use code on various platforms 
- <a href="http://xunit.github.io/">xUnit</a> tests on <a href="https://www.microsoft.com/net/core">.NET Core</a> with <a href="https://github.com/moq/moq4">Moq</a>
- <a href="https://autofac.org/">Autofac</a> for <a href="https://en.wikipedia.org/wiki/Dependency_injection">Dependency Injection</a>
- <a href="https://github.com/aspnet/EntityFrameworkCore">Entity Framework Core</a> to work with data
- <a href="http://www.sqlite.org/">SQLite</a> as a database engine
- <a href="https://www.newtonsoft.com/json">Newtonsoft.JSON</a> to parse JSON objects
- <a href="https://github.com/codehollow/FeedReader">CodeHollow.FeedReader</a> to parse feeds
- <a href="https://developer.microsoft.com/en-us/windows/apps">Universal Windows Platform</a>
- <a href="https://github.com/Microsoft/UWPCommunityToolkit">UWP Community Toolkit</a>
- <a href="https://github.com/Microsoft/XamlBehaviors">UWP XAML Behaviors SDK</a>
- <a href="https://code.visualstudio.com/">Visual Studio Code</a> with <a href="http://ionide.io/">Ionide plugin</a>
- <a href="https://www.visualstudio.com/ru/vs/whatsnew/">Visual Studio 2017</a> with <a href="https://www.jetbrains.com/resharper/">JetBrains ReSharper</a>

## License Info
Licensed under <a href="https://github.com/Worldbeater/myFeed/blob/master/LICENSE.md">MIT license</a>.
