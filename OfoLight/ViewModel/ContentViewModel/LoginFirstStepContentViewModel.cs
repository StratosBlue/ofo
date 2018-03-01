﻿using OfoLight.Entity;
using OfoLight.Utilities;
using OfoLight.View;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace OfoLight.ViewModel
{
    /// <summary>
    /// 登录第一步的内容VM
    /// </summary>
    public class LoginFirstStepContentViewModel : BaseContentViewModel
    {
        #region 字段

        private string _captchaCode;
        private bool _captchaCodeInputEnable = false;
        private string _telPhone;

        private BitmapImage _verifyCodeImage = new BitmapImage();

        #endregion 字段

        #region 属性

        /// <summary>
        /// 验证码
        /// </summary>
        public string CaptchaCode
        {
            get { return _captchaCode; }
            set
            {
                _captchaCode = value;
                NotifyPropertyChanged("CaptchaCode");
            }
        }

        /// <summary>
        /// 验证码可输入状态
        /// </summary>
        public bool CaptchaCodeInputEnable
        {
            get { return _captchaCodeInputEnable; }
            set
            {
                _captchaCodeInputEnable = value;
                NotifyPropertyChanged("CaptchaCodeInputEnable");
            }
        }

        /// <summary>
        /// 刷新验证码命令
        /// </summary>
        public ICommand RefreshVerifyCodeCommand { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string TelPhone
        {
            get { return _telPhone; }
            set
            {
                _telPhone = value;
            }
        }

        /// <summary>
        /// 验证码图片
        /// </summary>
        public BitmapImage VerifyCodeImage
        {
            get { return _verifyCodeImage; }
            set
            {
                _verifyCodeImage = value;
                NotifyPropertyChanged("VerifyCodeImage");
            }
        }

        /// <summary>
        /// 验证ID
        /// </summary>
        private string VerifyId { get; set; }

        #endregion 属性

        /// <summary>
        /// 登录第一步的内容VM
        /// </summary>

        #region 构造函数

        public LoginFirstStepContentViewModel()
        {
            CanExitApplication = true;
            RefreshVerifyCodeCommand = new RelayCommand(async (state) =>
            {
                CaptchaCode = string.Empty;
                await GetCaptchaCodeAsync();
            });
        }

        #endregion 构造函数

        #region 方法

        public async void InputCaptchaCodeTextChangedAsync(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Text.Length >= 4)
                {
                    CaptchaCode = tb.Text.Trim();
                    await SubmitCaptchaCodeAsync();
                    return;
                }
            }
        }

        protected override async Task InitializationAsync()
        {
            await GetCaptchaCodeAsync();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        private async Task GetCaptchaCodeAsync()
        {
            //VerifyCodeImage = VerifyCodeImage ?? new BitmapImage();
            //OfoApi.CurUser.TelPhone = TelPhone;
            var captchaCodeResult = await OfoApi.GetCaptchaCodeAsync();
            if (await CheckOfoApiResultHttpStatus(captchaCodeResult))
            {
                VerifyId = captchaCodeResult.Data.VerifyId;
                using (var imgStream = await AccessStreamUtility.GetRandomAccessStreamFormBase64String(captchaCodeResult.Data.CaptchaStr))
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        VerifyCodeImage.SetSource(imgStream);
                    });
                }
            }
        }

        /// <summary>
        /// 提交验证码
        /// </summary>
        /// <param name="state"></param>
        private async Task SubmitCaptchaCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(CaptchaCode))
            {
                return;
            }

            OfoApi.CurUser.TelPhone = TelPhone;
            var verifyCode = await OfoApi.GetVerifyCodeAsync(CaptchaCode.Trim(), VerifyId);
            if (await CheckOfoApiResult(verifyCode))
            {
                ContentPageArgs args = new ContentPageArgs()
                {
                    Name = "登录第二步",
                    HeaderVisibility = Visibility.Collapsed,
                    ContentElement = new LoginSecondStepContentView(),
                };

                ContentNavigation(args);
            }
        }

        #endregion 方法
    }
}