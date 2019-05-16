using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleSQL;

class PlayerTable
    {
        // The WeaponID field is set as the primary key in the SQLite database,
        // so we reflect that here with the PrimaryKey attribute
        [PrimaryKey]
        public int PlayerID { get; set; }

    [Default(0)]
    public int HighestLevelComplete { get; set; }
    [Default(0)]
    public int TotalDeaths { get; set; }
    [Default(0)]
    public int TotalWins { get; set; }
    [Default(0)]
    public int TotalGamesPlayed { get; set; }

    // calculated property
    [Default(0)]
    public int TotalLosses { get; set; }


}

