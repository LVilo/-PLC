using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Settings.Setting_4_20
{
    public interface ISettting_4_20
    {
        string ImageSettingOutput { get; }
        string ImageSettingInput { get;  }
        Task<bool> SetCurrent(float f, Window owner);
        void SetOutputSwtich(bool  swtich);
        float ReadCurrent();
    }
}
