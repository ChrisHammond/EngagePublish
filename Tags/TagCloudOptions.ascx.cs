//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


namespace Engage.Dnn.Publish.Tags
{
    using System;
    using System.Globalization;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;

    public partial class TagCloudOptions : ModuleSettingsBase
    {
        #region Event Handlers

        public override void LoadSettings()
        {
            try
            {
                chkLimitTagCount.Checked = PopularTagCount;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
       
        #endregion

        public override void UpdateSettings()
        {
            if (Page.IsValid)
            {
                    PopularTagCount = chkLimitTagCount.Checked;
            }
        }

        private bool PopularTagCount
        {
            set
            {
                var modules = new ModuleController();
                modules.UpdateTabModuleSetting(TabModuleId, "tcPopularTagBool", value.ToString(CultureInfo.InvariantCulture));
            }

            get
            {
                object o = Settings["tcPopularTagBool"];
                return (o == null || Convert.ToBoolean(o, CultureInfo.InvariantCulture));
            }
        }
    }
}

