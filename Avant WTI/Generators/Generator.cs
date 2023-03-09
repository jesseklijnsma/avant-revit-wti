using Avant.WTI.Data;

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
