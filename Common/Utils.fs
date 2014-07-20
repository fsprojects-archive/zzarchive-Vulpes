namespace Common

module Utils =
    open System

    let nextMultipleOf n i =
        let r = i % n
        if r = 0 then i else i + n - r

    let disposeAll ([<ParamArray>] arr : 'a list array when 'a :> IDisposable) =
        for items in arr do
            for item in items do item.Dispose()
