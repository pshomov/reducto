# Reducto is a port of [Redux](http://rackt.github.io/redux/) to .NET

[![Build Status](https://img.shields.io/travis/pshomov/reducto.svg?style=flat-square)](https://travis-ci.org/pshomov/reducto)
[![NuGet Release](https://img.shields.io/nuget/v/Reducto.svg?style=flat-square)](https://www.nuget.org/packages/Reducto/)

## What is Reducto?

Reducto is a keeper of the state for your app. It helps to organize the logic that changes that state. Really useful for GUI apps in combination with(but not limited to) MVVM, MVC, MVP etc. 

|Metric|Value|
|-------|-----|
|lines of code| ~260|
|dependencies| 0 |
|packaging | [NuGet PCL](https://www.nuget.org/packages/Reducto/) |

## Installation

In Package Manager Console run

```
PM> Install-Package Reducto
```

## Key concepts

 - **Action** - an object which describes what has happened - LoggedIn, SignedOut, etc. The object contains all the information relevant to the action - username, password, status, etc. Usually there are many actions in an app. 
 - **Reducer** - a side-effect free function that receives the current state of your app and an `action`. If the reducer does not know how to handle the `action` it should return the state as is. If the reducer can handle the `action` it 1.) makes a copy of the state 2.) it modifies it in response to the `action` and 3.) returns the copy.
 - **Store** - it is an object that contains your app's state. It also has a `reducer`. We _dispatch_ an `action` to the `store` which hands it to the `reducer` together with the current app state and then uses the return value of the `reducer` as the new state of the app. There is only one `store` in your app. It's created when your app starts and gets destroyed when your app quits. Your MVVM view models can _subscribe_ to be notified when the state changes so they can update themselves accordingly. 
 - **Async action** - a function that may have side effects. This is where you talk to your database, call a web service, navigate to a view model, etc. `Async actions` can also dispatch `actions` (as described above). To execute an `async action` it needs to be _dispatched_ to the `store`.
 - **Middleware** - these are functions that can be hooked in the `store` dispatch mechanism so you can do things like logging, profiling, authorization, etc. It's sort of a plugin mechanism which can be quite useful.

Dispatching an `action` to the store is **the only way to change its state**.<br>
Dispatching an `async action` cannot change the state but it can dispatch `actions` which in turn can change the state.

## How does one use this thing?

Here is a short example of Reducto in action. Let's write an app that authenticates an user. 

First, let's define the `actions` that we will need:

```c#
// Actions

public struct LoginStarted 
{ 
    public string Username; 
}

public struct LoginFailed {}

public struct LoginSucceeded 
{
    public string Token;
}
```
Next is the state of our app 

```c#
// State

public enum LoginStatus 
{
    LoginInProgress, LoggedIn, NotLoggedIn
}

public struct AppState
{
    public LoginStatus Status;
    public String Username;
    public String Token;
}
```

Here is how the `actions` change the state of the app

```c#
var reducer = new SimpleReducer<AppState>()
    .When<LoginStarted>((state, action) => {
        state.Username = action.Username;
        state.Token = "";
        state.Status = LoginStatus.LoginInProgress;
        return state;
    })
    .When<LoginSucceeded>((state, action) => {
        state.Token = action.Token;
        state.Status = LoginStatus.LoggedIn;
        return state;
    })
    .When<LoginFailed>((state, action) => {
        state.Status = LoginStatus.NotLoggedIn;
        return state;
    });

var store = new Store<AppState>(reducer);
```
Now let's take a moment to see what is going on here. We made a `reducer` using a builder and define how each `action` changes the state. This `reducer` is provieded to the `store` so the store can use it whenever an `action` is dispatched to it. Makes sense so far? I hope so ;)

Now let's see what is dispatching `actions` to the `store`. One can do that directly but more often then not it will be done from inside an `async action` like this one
```c#
var loginAsyncAction = store.asyncAction(async(dispatch, getState) => {
    dispatch(new LoginStarted{Username = "John Doe"});

    // faking authentication of user
    await Task.Delay(500);
    var authenticated = new Random().Next() % 2 == 0;

    if (authenticated) {
        dispatch(new LoginSucceeded{Token = "1234"});
    } else {
        dispatch(new LoginFailed());
    }
    return  authenticated;
});
```
A lot going on here. The `async action` gets a _dispatch_ and a _getState_ delegates. The latter one is not used in our case but the former is used a lot. We dispatch an action to signal the login process has started and then again after it has finished and depending on the outcome of the operation. How do we use this `async action`?
```c#
store.Dispatch(loginAsyncAction);
// or if you need to know the result of the login you can do also
var logged = await store.Dispatch(loginAsyncAction);
```

For more examples and please checkout the links below in the Resources section

## Resources

A couple of links on my blog
 
 - [Better MVVM with Xamarin Forms](http://pshomov.github.io/better-mvvm-with-xamarin-forms/)
 - [Compartmentalizing logic](http://pshomov.github.io/compartmentalizing-logic/)

## What about the name?

[It is pure magic ;-)](https://en.wikibooks.org/wiki/Muggles%27_Guide_to_Harry_Potter/Magic/Reducto#Overview)