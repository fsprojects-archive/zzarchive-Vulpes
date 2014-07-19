namespace Common

module Utils =

    let nextMultipleOf n i =
        let r = i % n
        if r = 0 then i else i + n - r
