using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ArkBot.ViewModel
{
    public abstract class PaneViewModel : ViewModelBase
    {
        public PaneViewModel(string contentId, string title)
        {
            ContentId = contentId;
            Title = title;
        }

        //public ImageSource IconSource { get; protected set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;

                _title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }
        private string _title;

        public string ContentId
        {
            get { return _contentId; }
            set
            {
                if (_contentId == value) return;
                _contentId = value;
                RaisePropertyChanged(nameof(ContentId));
            }
        }
        private string _contentId;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
        }
        private bool _isSelected;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;

                _isActive = value;
                RaisePropertyChanged(nameof(IsActive));
            }
        }
        private bool _isActive;
    }
}
