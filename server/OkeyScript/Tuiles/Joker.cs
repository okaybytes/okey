﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okey.Tuiles
{
    internal class Joker : Tuile
    {
        public Joker(CouleurTuile couleur, int valeur, bool dansPioche) : base(couleur, valeur, dansPioche)
        {
            //calculate value
        }


        public override bool MemeCouleur(Tuile t)
        {
            return this.Couleur == t.GetCouleur();
        }

        public override bool estSuivant(Tuile t)
        {
            return this.num + 1 == t.GetNum() || (this.num == 13 && t.GetNum() == 1);
        }

        public override String ToString()
        {
            return String.Format("({0:00}, {1}, {2})", this.num, this.Couleur, "Jo");
        }
    }
}