using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Configuration.Validation
{
    public class ValidationTemplate : /*IDataErrorInfo,*/ INotifyDataErrorInfo
    {
        private readonly INotifyPropertyChanged _target;
        readonly ValidationContext _validationContext;
        readonly List<ValidationResult> _validationResults;

        public ValidationTemplate(INotifyPropertyChanged target)
        {
            this._target = target;
            _validationContext = new ValidationContext(target, null, null);
            _validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(target, _validationContext, _validationResults, true);
            target.PropertyChanged += target_PropertyChanged;
        }

        void target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Validate();
        }

        public void Validate()
        {
            _validationResults.Clear();
            Validator.TryValidateObject(_target, _validationContext, _validationResults, true);

            var hashSet = new HashSet<string>(_validationResults.SelectMany(x => x.MemberNames));
            foreach (var error in hashSet)
            {
                RaiseErrorsChanged(error);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return _validationResults.Where(x => x.MemberNames.Contains(propertyName))
                                    .Select(x => x.ErrorMessage);
        }

        public bool HasErrors => _validationResults.Count > 0;

        public string Error
        {
            get
            {
                var strings = _validationResults.Select(x => x.ErrorMessage)
                                               .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public string this[string propertyName]
        {
            get
            {
                var strings = _validationResults.Where(x => x.MemberNames.Contains(propertyName))
                                               .Select(x => x.ErrorMessage)
                                               .ToArray();
                return string.Join(Environment.NewLine, strings);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
