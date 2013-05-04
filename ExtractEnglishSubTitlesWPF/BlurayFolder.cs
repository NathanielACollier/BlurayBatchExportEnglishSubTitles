using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractEnglishSubTitlesWPF
{
    public class BlurayFolder : WPFViewModelBase.ViewModelBase
    {

        public string Path
        {
            get { return base.GetValue(() => this.Path); }
            set { base.SetValue(() => this.Path, value); }
        }

    }
}
