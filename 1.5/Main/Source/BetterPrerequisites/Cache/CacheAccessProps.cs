﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigAndSmall
{
    public partial class BSCache
    {
        public Gender GetApparentGender() => apparentGender ?? pawn.gender;
    }
}