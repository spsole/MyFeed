module myFeed.Tests.Fixtures.SettingViewModel

open NSubstitute
open Xunit
open System
open System.Threading.Tasks
open myFeed.Interfaces
open myFeed.Models
open myFeed.Tests.Extensions
open myFeed.ViewModels

[<Theory>]
[<InlineData("light", 42., true, true, 42, 42)>]
[<InlineData("dark", 3., false, false, 1, 146)>]
let ``should load initial settings from store`` theme font banners images period max =

    let settingManager = Substitute.For<ISettingManager>()
    settingManager.Read().Returns(
        Settings( 
            Max = max,
            Font = font, 
            Theme = theme,
            Banners = banners, 
            Images = images, 
            Period = period) 
        |> Task.FromResult) 
        |> ignore 

    let settingViewModel = produce<SettingViewModel> [settingManager]
    settingViewModel.Load.Invoke().Wait()
    
    Should.equal max settingViewModel.Max
    Should.equal font settingViewModel.Font
    Should.equal theme settingViewModel.Theme
    Should.equal banners settingViewModel.Banners
    Should.equal images settingViewModel.Images
    Should.equal period settingViewModel.Period
