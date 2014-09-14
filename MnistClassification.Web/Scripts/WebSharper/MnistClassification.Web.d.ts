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
            var LoadMnist : {
                (k: {
                    (x: string): void;
                }): void;
            };
            var TrainMnistUnsupervised : {
                (learningRate: string, momentum: string, batchSize: string, epochs: string, k: {
                    (x: string): void;
                }): void;
            };
            var MnistControls : {
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
