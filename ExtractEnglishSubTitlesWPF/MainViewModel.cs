using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ExtractEnglishSubTitlesWPF
{
    public class MainViewModel : WPFViewModelBase.ViewModelBase
    {


        public string Message
        {
            get { return base.GetValue(() => this.Message); }
            set { base.SetValue(() => this.Message, value); }
        }


        public string SubTitlesFolder
        {
            get { return base.GetValue(() => this.SubTitlesFolder); }
            set { base.SetValue(() => this.SubTitlesFolder, value); }
        }

        public ObservableCollection<BlurayFolder> Folders
        {
            get { return base.GetValue(() => this.Folders); }
        }


    }
}
