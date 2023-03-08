using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Generators
{
    abstract class Generator
    {

        protected readonly WTIData data;

        public Generator(WTIData data)
        {
            this.data = data;
        }

        public abstract void GeneratePreview();
        public abstract bool GenerateModel();

    }
}
