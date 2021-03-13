using System;
using System.Collections.Generic;
using System.Text;

namespace Core.LocalData
{
    public static class Scripts
    {
        public static List<string> List = new List<string>(2) {
            @"
CREATE TABLE Mod (
    Id      INTEGER         PRIMARY KEY ASC AUTOINCREMENT
                            NOT NULL,
    Name    VARCHAR (60)    NOT NULL,
    Hash    VARCHAR         NOT NULL,
    Version VARCHAR (14, 0),
    Author  VARCHAR (150)
);
",
            @"
CREATE TABLE ModChange (
    Id      INTEGER       PRIMARY KEY AUTOINCREMENT
                          NOT NULL,
    ModId   INTEGER       CONSTRAINT Mod_Change_FK REFERENCES Mod (Id) ON DELETE CASCADE,
    Type    INTEGER       NOT NULL,
    Section VARCHAR (150) NOT NULL,
    [Key]   VARCHAR (150) NOT NULL,
    OldVal  VARCHAR (150) NOT NULL,
    NewVal  VARCHAR (150) NOT NULL,
    State   INTEGER       NOT NULL
);

"
        };
    }
}
