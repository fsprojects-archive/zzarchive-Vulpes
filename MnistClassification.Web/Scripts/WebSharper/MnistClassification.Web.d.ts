declare module MnistClassification {
    module Web {
        module Skin {
            interface Page {
                Title: string;
                Body: __ABBREV.__List.T<any>;
            }
        }
        module Controls {
            interface EntryPoint {
                get_Body(): __ABBREV.__Html.IPagelet;
            }
        }
        module Client {
            interface Margin {
                Top: number;
                Right: number;
                Bottom: number;
                Left: number;
            }
            var Start : {
                (input: string, k: {
                    (x: string): void;
                }): void;
            };
            var Main : {
                (): __ABBREV.__Html.Element;
            };
            var TrainingSet : {
                (): __ABBREV.__Html.Element;
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
    export import __Html = IntelliFactory.WebSharper.Html;
}
