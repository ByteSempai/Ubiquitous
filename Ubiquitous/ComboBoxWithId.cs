using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ubiquitous
{
    public class ComboBoxWithId : ComboBox
    {
        public class ComboBoxItem
        {

            private string title_;
            private UInt32 id_;

            public ComboBoxItem(string title, UInt32 id)
            {
                title_ = title;
                id_ = id;
            }

            public UInt32 id
            {
                get
                {
                    return id_;
                }
            }
            public override string ToString()
            {
                return title_;
            }
        }

        delegate void AddComboBoxItem_(ComboBoxItem obj);
        delegate void ClearComboBox();
        delegate void SetComboDataSource(BindingSource source,string displayMember, string valueMember);
        public ComboBoxWithId()
        {
        }
        public void SetDataSource(BindingSource source, string displayMember = "", string valueMember = "")
        {
            if (this.InvokeRequired)
            {
                SetComboDataSource dlgt = new SetComboDataSource(SetDataSource);
                Invoke(dlgt, new object[] { source,displayMember,ValueMember });
            }
            else
            {
                if (source == null)
                    this.DataSource = null;
                else
                    this.DataSource = source.DataSource;

                this.DisplayMember = displayMember;
                this.ValueMember = ValueMember;

            }
        }
        public void Clear()
        {
            if (this.InvokeRequired)
            {
                ClearComboBox dlgt = new ClearComboBox(Clear);
                Invoke(dlgt);
            }
            else
            {
                this.Items.Clear();
            }
        }
        public void AddItem(ComboBoxItem obj)
        {
            if (this.InvokeRequired)
            {
                AddComboBoxItem_ dlgt = new AddComboBoxItem_(AddItem);
                Invoke(dlgt, new object[] { obj });
            }
            else
            {
                this.Items.Add(obj);
            }
        }

    }



}
