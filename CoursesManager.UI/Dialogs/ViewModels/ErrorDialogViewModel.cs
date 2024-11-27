using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesManager.UI.Dialogs.ResultTypes;

namespace CoursesManager.UI.Dialogs.ViewModels
{
    public class ErrorDialogViewModel(DialogResultType? initialData)
        : NotifyDialogViewModel(initialData);
}