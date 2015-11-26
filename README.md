# Reducto is a port of [Redux](http://rackt.github.io/redux/) to .NET

[![Build Status](https://img.shields.io/travis/pshomov/reducto.svg?style=flat-square)](https://travis-ci.org/pshomov/reducto)
[![NuGet Release](https://img.shields.io/nuget/v/Reducto.svg?style=flat-square)](https://www.nuget.org/packages/Reducto/)

Reducto is a keeper of the state for your app. It helps to organize the logic that changes that state.

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

## Flow

Coming soon ...

## Resources

A couple of links on my blog
 
 - [Better MVVM with Xamarin Forms](http://pshomov.github.io/better-mvvm-with-xamarin-forms/)
 - [Compartmentalizing logic](http://pshomov.github.io/compartmentalizing-logic/)
