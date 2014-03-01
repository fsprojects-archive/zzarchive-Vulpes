declare module MnistClassification {
    module Web {
        module Skin {
            interface Page {
                Title: string;
                Body: __ABBREV.__List.T<__ABBREV.__Html.Element<__ABBREV.__Web.Control>>;
            }
        }
        module Controls {
            interface EntryPoint {
                get_Body(): __ABBREV.__Html1.IPagelet;
            }
        }
        module Client {
            var Start : {
                (input: string, k: {
                    (x: string): void;
                }): void;
            };
            var Main : {
                (): __ABBREV.__Html1.Element;
            };
        }
        interface Action {
        }
        interface Website {
        }
    }
}
declare module __ABBREV {
    
    export import __List = IntelliFactory.WebSharper.List;
    export import __Html = IntelliFactory.Html.Html;
    export import __Web = IntelliFactory.WebSharper.Web;
    export import __Html1 = IntelliFactory.WebSharper.Html;
}
