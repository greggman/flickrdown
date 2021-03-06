using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using FlickrNet;

namespace FlickrDown
{
    [ComVisible(true)]
    public partial class MainForm : Form
    {
        private const string appkey = "142daee489f7fa7ee1cde6647d9c66d1";
        private const string sharedSecret = "45cd580567c81986";
        private const string reg_path = "Software\\Greggman\\FlickrDown\\Settings";
        private const int PhotosPerPage = 500;
        private Flickr _fapi = null;
        private FoundUser _curUser = null;
        private Person _curPerson = null;
        private Photosets _curPhotosets = null;
        private Photos _curPhotos = null;

        private GroupSearchResults _curPoolGroups = null;
        private Photos[] _curGroupPhotos = null;

        private string _curTags = "";
        private Photos _curTagPhotos = null;

        private List<DLPhoto> _curDLPhotos = null;
        private BackgroundWorker _bgw = null;
        private StatusForm _statusForm;

        private bool _useProxy = false;
        private string _proxyAddress = "";
        private bool _useProxyAuth = false;
        private string _proxyUsername = "";
        private string _proxyPassword = "";

        private int _downloadLimit = 500;

        private string _searchInfo = "";
        private string _searchType = "username";
        private string _destFolder = "";
        private string _flickrToken = "";

        // yea, I know I should make a class for this
        /*
        class HistoryString
        {
            private const int    maxHistory = 10;
            private string       _value
            private string       _controlName;
            private List<string> _history = new List<string>;

            public int Count
            {
                get { return _history.Count(); }
            }
            public int Value
            {
                get { return _value; }
                set { _value = value; }
            }
            public string GetAt(int index)
            {
                return _history[ii];
            }
            public SetAt(int index, string value)
            {
                _history[ii] = value;
            }
        }
        */
        private const string _destCtrlName = "destfolder";
        private List<string> _destHistory = new List<string>();
        private const string _searchCtrlName = "searchinfo";
        private List<string> _searchHistory = new List<string>();

        // yes I know I should have used some object oriented way to do this but
        // that required thought or in otherwords time and I needed to get this
        // done ASAP.  Refactor if it's actually a real issue in the future
        enum BrowseType
        {
            photosets,
            tags,
            groups,
        }

        private BrowseType _browseType;

        public MainForm()
        {
            InitializeComponent();

            // setup historys
            for (int ii = 0; ii < 10; ++ii)
            {
                _destHistory.Add("");
                _searchHistory.Add("");
            }

            // Attempt to open the key
            RegistryKey key = Registry.CurrentUser.OpenSubKey( reg_path );

            // If the return value is null, the key doesn't exist
            if ( key == null )
            {
                // The key doesn't exist; create it / open it
                key = Registry.CurrentUser.CreateSubKey( reg_path );
            }

            UpdateReg(key, false);
            UpdateProxy();

            this.webBrowser1.ObjectForScripting = this;

            _fapi = new Flickr(appkey, sharedSecret);
            _fapi.AuthToken = _flickrToken;
            ValidateToken();

            // setup background worker
            _bgw = new BackgroundWorker();
            _bgw.WorkerReportsProgress = true;
            _bgw.WorkerSupportsCancellation = true;
            _bgw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_DoWork);
            _bgw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_RunWorkerCompleted);
            _bgw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgw_ProgressChanged);

            // load up html form
            string formurl = "mainform.html";

            Util.FindLocalFile(ref formurl);
            this.webBrowser1.Navigate(formurl);
        }

        private void UpdateHistory(ref List<string> history, string controlName)
        {
            ClearOptions(controlName);
            int count = 0;
            for (int ii = 0; ii < history.Count; ++ii)
            {
                if (history[ii].Length > 0)
                {
                    count++;
                    AddOption(controlName, history[ii], history[ii]);
                }
            }
        }

        private void AddToHistory(ref List<string> history, string controlName, string newValue)
        {
            if (!history.Contains(newValue))
            {
                history.RemoveAt(0);
                history.Add(newValue);
                UpdateHistory(ref history, controlName);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            UpdateHistory(ref _destHistory, _destCtrlName);
            UpdateHistory(ref _searchHistory, _searchCtrlName);
            SetValue("searchinfo", _searchInfo);
            SetRadioValue("searchtype", _searchType);
            SetValue("destfolder", _destFolder);
            UpdateAuthenticateButton();
        }

        private void UpdateAuthenticateButton()
        {
            if (_flickrToken.Length > 0)
            {
                SetValue("authenticate", "De-authorize");
            }
            else
            {
                SetValue("authenticate", "Authorize...");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey( reg_path, true );

            UpdateReg(key, true);
        }

        private void ValidateToken()
        {
            if (_flickrToken.Length > 0)
            {
                try
                {
                    _fapi.AuthCheckToken(_flickrToken);
                }
                catch (Exception)
                {
                    _flickrToken = "";
                    _fapi.AuthToken = "";
                }
            }
        }

        private void UpdateKey (RegistryKey key, string label, ref bool value, bool bSave)
        {
            if (bSave)
            {
                key.SetValue(label, value.ToString());
            }
            else
            {
                if (key.GetValue(label) != null)
                {
                    value = bool.Parse(key.GetValue(label).ToString());
                }
            }
        }

        private void UpdateKey (RegistryKey key, string label, ref string value, bool bSave)
        {
            if (bSave)
            {
                key.SetValue(label, value.ToString());
            }
            else
            {
                if (key.GetValue(label) != null)
                {
                    value = key.GetValue(label).ToString();
                }
            }
        }

        private void UpdateKey (RegistryKey key, string label, ref int value, bool bSave)
        {
            if (bSave)
            {
                key.SetValue(label, value.ToString());
            }
            else
            {
                if (key.GetValue(label) != null)
                {
                    value = int.Parse(key.GetValue(label).ToString());
                }
            }
        }


        private void UpdateListKey(RegistryKey key, ref List<string> history, string controlName, bool bSave)
        {
            for (int ii = 0; ii < history.Count; ++ii)
            {
                string str = history[ii];
                UpdateKey(key, controlName + "_" + ii.ToString(), ref str, bSave);
                if (!bSave)
                {
                    history[ii] = str;
                }
            }
        }

        private void UpdateReg (RegistryKey key, bool bSave)
        {
            UpdateKey (key, "useProxy",       ref _useProxy      , bSave);
            UpdateKey (key, "proxyAddress",   ref _proxyAddress  , bSave);
            UpdateKey (key, "useProxyAuth",   ref _useProxyAuth  , bSave);
            UpdateKey (key, "proxyUsername",  ref _proxyUsername , bSave);
            UpdateKey (key, "proxyPassword",  ref _proxyPassword , bSave);
            UpdateKey (key, "searchInfo",     ref _searchInfo    , bSave);
            UpdateKey (key, "searchType",     ref _searchType    , bSave);
            UpdateKey (key, "destFolder",     ref _destFolder    , bSave);
            UpdateKey (key, "token",          ref _flickrToken   , bSave);
            UpdateKey (key, "downloadLimit",  ref _downloadLimit , bSave);

            UpdateListKey(key, ref _destHistory, _destCtrlName, bSave);
            UpdateListKey(key, ref _searchHistory, _searchCtrlName, bSave);
        }

        private void UpdateProxy()
        {
            if (_useProxy)
            {
                Uri proxyURI   = new Uri(_proxyAddress);
                WebProxy proxy = new WebProxy(proxyURI, true);

                if (_useProxyAuth)
                {
                    proxy.Credentials = new NetworkCredential(
                        _proxyUsername,
                        _proxyPassword);
                }

                //GlobalProxySelection.Select = proxy; // 2.0 beta 1
                WebRequest.DefaultWebProxy = proxy; // 2.0 beta 2
            }
            else
            {
                // GlobalProxySelection.Select = GlobalProxySelection.GetEmptyWebProxy(); // 2.0 beta 1
                WebRequest.DefaultWebProxy = null; // 2.0 beta 2
            }
        }

        private void FindUser(string username)
        {
            if (username.IndexOf('@') >= 0)
            {
                _curUser = _fapi.PeopleFindByEmail(username);
            }
            else
            {
                _curUser = _fapi.PeopleFindByUsername(username);
            }
        }


        private void LookupUser(string name)
        {
            FindUser(name);
            _curPerson = _fapi.PeopleGetInfo(_curUser.UserId);
        }

        private void AddItem(bool bCheck, string title, string description, bool bOpen)
        {
            Object[] args = new Object[] { bCheck, title, description, bOpen, };

            this.webBrowser1.Document.InvokeScript("additem", args);
        }

        private void GetPhotosets()
        {
            try
            {
                _curPhotosets = _fapi.PhotosetsGetList(_curUser.UserId);
                _curPhotos = null;

                if (_curPhotosets.PhotosetCollection != null)
                {
                    foreach (Photoset ps in _curPhotosets.PhotosetCollection)
                    {
                        AddItem(false, ps.Title, ps.Description, false);
                    }
                }
                AddItem(false, "[*All_Photos*]", "All public photos including those not in a photo set", false);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void ClearPhotosets()
        {
            Object[] args = new Object[] { "listtbody", };
            this.webBrowser1.Document.InvokeScript("clearlist", args);
        }

        private void GetGroups(string text)
        {
            try
            {
                _curPoolGroups = _fapi.GroupsSearch(text);
                _curGroupPhotos = null;

                if (_curPoolGroups.Groups != null)
                {
                    _curGroupPhotos = new Photos[_curPoolGroups.Groups.Count];
                    foreach (GroupSearchResult gsr in _curPoolGroups.Groups)
                    {
                        AddItem(false, gsr.GroupName, "", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        // Reallocates an array with a new size, and copies the contents
        // of the old array to the new array.
        // Arguments:
        //   oldArray  the old array, to be reallocated.
        //   newSize   the new array size.
        // Returns     A new array with the same contents.
        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        // this is really FUCKING STUPID.
        // sometime between when I started this project and now Photos went from
        // using a PhotoCollection to using a Photo[]. Well. A Photo[] is a static size.
        // You can't reallocate it. Then on top of that. 
        public Photo[] ResizePhotoCollection(Photo[] pc, int newSize)
        {
            Photo[] newpc = new Photo[newSize];

            for (int ii = 0; ii < pc.Length; ++ii)
            {
                newpc[ii] = pc[ii];
            }

            for (int ii = pc.Length; ii < newSize; ++ii)
            {
                newpc[ii] = new Photo();
            }

            return newpc;
        }

        public Photos GetTagPhotos(string tags)
        {
            Photos allPhotos = new Photos();
            Photos ps = _fapi.PhotosSearch(tags, TagMode.AllTags, "", 1, 1);

            int numToGet = Math.Min((int)ps.TotalPhotos, _downloadLimit);
            int pages    = (numToGet + PhotosPerPage - 1) / PhotosPerPage;

            // put them in a list because the number may have changed between the time we got this info and now
            for (int page = 1; page <= pages; page++)
            {
                ps = _fapi.PhotosSearch(tags, TagMode.AllTags, "", PhotosPerPage, page);
                foreach (Photo ph in ps.PhotoCollection)
                {
                    //allPhotos.PhotoCollection.Add(ph);

                    allPhotos.PhotoCollection = ResizePhotoCollection(allPhotos.PhotoCollection, allPhotos.PhotoCollection.Length + 1);
                    allPhotos.PhotoCollection[allPhotos.PhotoCollection.Length - 1] = ph;

                    if (allPhotos.PhotoCollection.Length >= numToGet)
                    {
                        break;
                    }
                }
            }

            return allPhotos;
        }

        private void GetPhotosByTag(string tags)
        {
            try
            {
                _curTags = tags;
                _curTagPhotos = GetTagPhotos(tags);
                if (_curTagPhotos.PhotoCollection.Length == 0)
                {
                    MessageBox.Show(this, "There are no photos that match these tags");
                }
                else
                {
                    AddItem(false, tags, "", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private string GetCheck(string name)
        {
            Object[] args = new Object[] { name };

            return this.webBrowser1.Document.InvokeScript("getcheck", args).ToString();
        }

        public void SetValue(string name, string text)
        {
            Object[] args = new Object[] { name, text };

            this.webBrowser1.Document.InvokeScript("setvalue", args);
        }

        public void ClearOptions(string name)
        {
            Object[] args = new Object[] { name };

            this.webBrowser1.Document.InvokeScript("clearoptions", args);
        }

        public void AddOption(string name, string text, string value)
        {
            Object[] args = new Object[] { name, text, value, false };

            this.webBrowser1.Document.InvokeScript("addoption", args);
        }

        public void SetRadioValue(string name, string valueToBeChecked)
        {
            Object[] args = new Object[] { name, valueToBeChecked };

            this.webBrowser1.Document.InvokeScript("SetRadioValue", args);
        }

        private string removeSpaces(string str)
        {
            return Regex.Replace(str, " ", "");
        }

        private void ConfirmOrMakeFolder (string path)
        {
            if (path.Length > 3)
            {
                if (!Directory.Exists(path))
                {
                    ConfirmOrMakeFolder(Path.GetDirectoryName(path));
                    Directory.CreateDirectory(path);
                }
            }
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrefsForm frm = new PrefsForm();

            frm.UseProxy = _useProxy;
            frm.ProxyAddress = _proxyAddress;
            frm.UseProxyAuth = _useProxyAuth;
            frm.ProxyUsername = _proxyUsername;
            frm.ProxyPassword = _proxyPassword;
            frm.DownloadLimit = _downloadLimit;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                 _useProxy =      frm.UseProxy;
                 _proxyAddress =  frm.ProxyAddress;
                 _useProxyAuth =  frm.UseProxyAuth;
                 _proxyUsername = frm.ProxyUsername;
                 _proxyPassword = frm.ProxyPassword;
                 _downloadLimit = frm.DownloadLimit;

                 UpdateProxy();
            }
            frm.Dispose();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm frm = new AboutForm();
            frm.ShowDialog();
            frm.Dispose();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string helpfile = "help.html";
            if (Util.FindLocalFile(ref helpfile))
            {
                System.Diagnostics.Process.Start(Path.GetFullPath(helpfile));
            }
            else
            {
                MessageBox.Show(this,"could not display help : " + helpfile);
            }

        }

        private bool GetSrcURLByLabel(ref string srcURL, Sizes sz, string label)
        {
            foreach (FlickrNet.Size s in sz.SizeCollection)
            {
                if (s.Label == label)
                {
                    srcURL = s.Source;
                    return true;
                }
            }

            return false;
        }

        private string GetSrcURL(string photoId)
        {
            string srcURL = "";
            Sizes sz = _fapi.PhotosGetSizes(photoId);

            if (GetSrcURLByLabel(ref srcURL, sz, "Original")) { return srcURL; }
            if (GetSrcURLByLabel(ref srcURL, sz, "Large")) { return srcURL; }
            if (GetSrcURLByLabel(ref srcURL, sz, "Medium")) { return srcURL; }
            if (GetSrcURLByLabel(ref srcURL, sz, "Small")) { return srcURL; }

            return "no url found"; // TODO: throw exception
        }


        public void DownloadPhotos (BackgroundWorker bgw)
        {
            Regex r = new Regex("-fd(?<1>\\d\\d\\d\\d)$", RegexOptions.Compiled);
            WebClient client = new WebClient ();
            {
                int index = 0;
                foreach (DLPhoto dlp in _curDLPhotos)
                {
					if (bgw.CancellationPending)
					{
						return;
					}

                    // get the real info
                    PhotoInfo pi = _fapi.PhotosGetInfo(dlp.photo.PhotoId, dlp.photo.Secret);

                    string dstFolder = Path.Combine(_destFolder, Util.MakeFilenameSafe(dlp.photoSetName));

					try
					{
						ConfirmOrMakeFolder(dstFolder);
					}
					catch (Exception)
					{
						dstFolder = Path.Combine(_destFolder, Util.MakeFilenameSuperSafe(dlp.photoSetName));
					}

////                  string src  = "http://photos" + dlp.photo.Server + ".flickr.com/" + dlp.photo.PhotoId + "_" + dlp.photo.Secret + "_o.jpg";
////                  string src  = dlp.photo.OriginalUrl;
//                    string src = "http://static.flickr.com/" + dlp.photo.Server + "/" + dlp.photo.PhotoId + "_" + dlp.photo.Secret + "_o." + origExt;
                    string origExt = "jpg";
                    if (pi.OriginalFormat != null && pi.OriginalFormat.Length > 0)
                    {
                        origExt = dlp.photo.OriginalFormat;
                    }
                    // string src = pi.OriginalUrl;
                    string src = GetSrcURL(dlp.photo.PhotoId);
                    string dst = Path.Combine(dstFolder, Util.MakeFilenameSafe(dlp.photo.Title));
                    string ext = "." + origExt;
                    if (Path.GetExtension(dst).ToLower().CompareTo(ext) != 0)
                    {
                        dst = dst + ext;
                    }

					if (!File.Exists(dst))
					{
						// it doesn't exist therefore we don't know if this is a valid filename
						try
						{
							// create creating the file then delete it
							// if we are succesful there's not problem
							StreamWriter sw = new StreamWriter(dst);
							sw.Close();
							File.Delete(dst);
						}
						catch (Exception)
						{
							// create a super safe name
							dst = Path.Combine(dstFolder, Util.MakeFilenameSuperSafe(dlp.photo.Title));
							if (Path.GetExtension(dst).ToLower().CompareTo(ext) != 0)
							{
								dst = dst + ext;
							}
						}
					}

                    while (File.Exists(dst))
                    {
                        string dstPath = Path.GetDirectoryName(dst);
                        string dstName = Path.GetFileNameWithoutExtension(dst);
                        string dstExt  = Path.GetExtension(dst);

                        // either add the FlickrDown extension or increment it
                        Match m = r.Match(dstName);
                        if (m.Success)
                        {
                            // already have fd extension so increment it
                            string numStr = m.Groups[1].ToString();
                            int num = int.Parse(numStr);
                            dst = Path.Combine(dstPath, dstName.Substring(0, dstName.Length - 4) +
                                   String.Format("{0:d4}", num + 1) + dstExt);
                        }
                        else
                        {
                            dst = Path.Combine(dstPath, dstName + "-fd0000" + dstExt);
                        }
                    }

					//{
                    //    StreamWriter sw = new StreamWriter(@"flickrdebuglog.txt", true);
                    //
					//	sw.Write("(");
					//	sw.Write(src);
					//	sw.Write(")->(");
					//	sw.Write(dst);
					//	sw.Write(")\n");
					//	sw.Close();
					//}

                    client.DownloadFile(src, dst);

                    bgw.ReportProgress(0, index);

                    index++;
                }
            }
            client.Dispose();
        }

        // ********************************** status interface ***********************************

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            // This method will run on a thread other than the UI thread.
            // Be sure not to manipulate any Windows Forms controls created
            // on the UI thread from this method.
            BackgroundWorker bgw = sender as BackgroundWorker;
            //bgw.e = e;
            //bgw.e.Result = "";
            DownloadPhotos(bgw);
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int ndx = (int)e.UserState;

            _statusForm.CheckOff(ndx);
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // this runs in the form's thread.  Close the dialog I guess
            _statusForm.Close();
            _statusForm.Dispose();
            Cursor = Cursors.Default;
            this.Enabled = true;

            if (e.Error != null)
            {
                MessageBox.Show(this, e.Error.Message);
            }
            else if (e.Cancelled || _bgw.CancellationPending)
            {
                // Next, handle the case where the user canceled
                // the operation.
                // Note that due to a race condition in
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                MessageBox.Show(this, "Cancelled");
            }
            else
            {
                // Finally, handle the case where the operation
                // succeeded.
                //result.Text = e.Result.ToString();
                MessageBox.Show(this, "Finished");
            }
        }

        // ********************************** html interface ***********************************

        public void GetNewPhotosets(string searchinfo, string searchtype)
        {
            _searchInfo = searchinfo;
            _searchType = searchtype;
            ClearPhotosets();
            try
            {
                if (searchtype.CompareTo("username") == 0)
                {
                    _browseType = BrowseType.photosets;
                    LookupUser(searchinfo);
                    GetPhotosets();
                }
                else if (searchtype.CompareTo("e-mail") == 0)
                {
                    _browseType = BrowseType.photosets;
                    LookupUser(searchinfo);
                    GetPhotosets();
                }
                else if (searchtype.CompareTo("tags") == 0)
                {
                    _browseType = BrowseType.tags;
                    GetPhotosByTag(searchinfo);
                }
                else if (searchtype.CompareTo("group") == 0)
                {
                    _browseType = BrowseType.groups;
                    GetGroups(searchinfo);
                }

                AddToHistory(ref _searchHistory, _searchCtrlName, searchinfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        public string GetFolder(string oldFolder)
        {
            this.folderBrowserDialog1.SelectedPath = oldFolder;
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                return this.folderBrowserDialog1.SelectedPath;
            }

            return "";
        }

        public Photo[] GetPhotosetPhotos(Photoset ps)
        {
            int numToGet = Math.Min(ps.NumberOfPhotos, _downloadLimit);
            int pages    = (numToGet + PhotosPerPage - 1) / PhotosPerPage;

            List<Photo> allPhotos = new List<Photo>();

            // put them in a list because the number may have changed between the time we got this info and now
            for (int page = 1; page <= pages; page++)
            {
                Photo[] photos = _fapi.PhotosetsGetPhotos(ps.PhotosetId, page, PhotosPerPage);

                foreach (Photo ph in photos)
                {
                    allPhotos.Add(ph);
                    if (allPhotos.Count >= numToGet)
                    {
                        break;
                    }
                }
            }

            // convert the list to an array
            Photo[] pc = new Photo[allPhotos.Count];
            int index = 0;
            foreach(Photo ph in allPhotos)
            {
                pc[index] = ph;
                index++;
            }

            return pc;
        }

        public Photos GetUsersPhotos(FoundUser user)
        {
            Photos allPhotos = new Photos();
            Photos ps = _fapi.PhotosSearch(user.UserId, "", TagMode.AllTags, "", DateTime.MinValue, DateTime.MinValue, 0, 1, 1, PhotoSearchExtras.All);

            int numToGet = Math.Min((int)ps.TotalPhotos, _downloadLimit);
            int pages    = (numToGet + PhotosPerPage - 1) / PhotosPerPage;

            // put them in a list because the number may have changed between the time we got this info and now
            for (int page = 1; page <= pages; page++)
            {
                ps = _fapi.PhotosSearch(user.UserId, "", TagMode.AllTags, "", DateTime.MinValue, DateTime.MinValue, 0, PhotosPerPage, page, PhotoSearchExtras.All);
                foreach (Photo ph in ps.PhotoCollection)
                {
                    //allPhotos.PhotoCollection.Add(ph);
                    allPhotos.PhotoCollection = ResizePhotoCollection(allPhotos.PhotoCollection, allPhotos.PhotoCollection.Length + 1);
                    allPhotos.PhotoCollection[allPhotos.PhotoCollection.Length - 1] = ph;

                    if (allPhotos.PhotoCollection.Length >= numToGet)
                    {
                        break;
                    }
                }
            }

            return allPhotos;
        }

        public Photos GetGroupPhotos(GroupSearchResult gsr)
        {
            Photos allPhotos = new Photos();
            Photos ps = _fapi.GroupPoolGetPhotos(gsr.GroupId, 1, 1);

            int numToGet = Math.Min((int)ps.TotalPhotos, _downloadLimit);
            int pages    = (numToGet + PhotosPerPage - 1) / PhotosPerPage;

            // put them in a list because the number may have changed between the time we got this info and now
            for (int page = 1; page <= pages; page++)
            {
                ps = _fapi.GroupPoolGetPhotos(gsr.GroupId, PhotosPerPage, page);
                foreach (Photo ph in ps.PhotoCollection)
                {
                    // allPhotos.PhotoCollection.Add(ph);
                    allPhotos.PhotoCollection = ResizePhotoCollection(allPhotos.PhotoCollection, allPhotos.PhotoCollection.Length + 1);
                    allPhotos.PhotoCollection[allPhotos.PhotoCollection.Length - 1] = ph;

                    if (allPhotos.PhotoCollection.Length >= numToGet)
                    {
                        break;
                    }
                }
            }

            return allPhotos;
        }

        public void DownloadSets(string destFolder)
        {
            _destFolder  = destFolder;
            _curDLPhotos = new List<DLPhoto>();
            try
            {
                if (_browseType == BrowseType.photosets)
                {
                    int numSets = 0;
                    if (_curPhotosets.PhotosetCollection != null)
                    {
                        numSets = _curPhotosets.PhotosetCollection.Length;
                    }
                    for (int ii = 0; ii <= numSets; ++ii)
                    {
                        Photo[] pc = null;
                        Photoset ps = null;
                        string title = null;

                        if (ii >= numSets)
                        {
                            title = "All Photos";
                            if (_curPhotos != null)
                            {
                                pc = _curPhotos.PhotoCollection;
                            }
                        }
                        else
                        {
                            ps = _curPhotosets.PhotosetCollection[ii];
                            pc = ps.PhotoCollection;
                            title = ps.Title;
                        }

                        bool bChecked = (String.Compare(GetCheck("postset4_" + ii), "1") == 0);
                        bool bGetSelected = (!bChecked && pc != null);

                        // if checked downlaad ALL
                        // if not checked and photoCollection != null download selected

                        if (pc == null && bChecked)
                        {
                            if (ps != null)
                            {
                                pc = GetPhotosetPhotos(ps);
                            }
                            else
                            {
                                _curPhotos = GetUsersPhotos(_curUser);
                                pc = _curPhotos.PhotoCollection;
                            }
                        }

                        if (pc != null && (bChecked || bGetSelected))
                        {
                            int photoNdx = 0;
                            foreach (Photo ph in pc)
                            {
                                bool bSelected = false;
                                if (bGetSelected)
                                {
                                    bSelected = (String.Compare(GetCheck(String.Format("check{0:d5}_{1}", ii, photoNdx)), "1") == 0);
                                }

                                if (bChecked || bSelected)
                                {
                                    _curDLPhotos.Add(new DLPhoto(ph, title));
                                }

                                photoNdx++;
                            }
                        }
                    }
                }
                else if (_browseType == BrowseType.tags)
                {
                    Photo[] pc = _curTagPhotos.PhotoCollection;
                    string title = _curTags;

                    bool bChecked = (String.Compare(GetCheck("postset4_" + 0), "1") == 0);
                    bool bGetSelected = (!bChecked && pc != null);

                    if (pc != null && (bChecked || bGetSelected))
                    {
                        int photoNdx = 0;
                        foreach (Photo ph in pc)
                        {
                            bool bSelected = false;
                            if (bGetSelected)
                            {
                                bSelected = (String.Compare(GetCheck(String.Format("check{0:d5}_{1}", 0, photoNdx)), "1") == 0);
                            }

                            if (bChecked || bSelected)
                            {
                                _curDLPhotos.Add(new DLPhoto(ph, title));
                            }

                            photoNdx++;
                        }
                    }
                }
                else if (_browseType == BrowseType.groups)
                {
                    int numSets = 0;
                    if (_curPoolGroups != null)
                    {
                        numSets = _curPoolGroups.Groups.Count;
                    }
                    for (int ii = 0; ii < numSets; ++ii)
                    {
                        Photo[] pc = null;
                        GroupSearchResult gsr = null;
                        string title = null;

                        gsr = _curPoolGroups.Groups[ii];
                        title = gsr.GroupName;

                        if (_curGroupPhotos[ii] != null)
                        {
                            pc = _curGroupPhotos[ii].PhotoCollection;
                        }

                        bool bChecked = (String.Compare(GetCheck("postset4_" + ii), "1") == 0);
                        bool bGetSelected = (!bChecked && pc != null);

                        // if checked downlaad ALL
                        // if not checked and photoCollection != null download selected

                        if (pc == null && bChecked)
                        {
                            _curGroupPhotos[ii] = GetGroupPhotos(gsr);
                            pc = _curGroupPhotos[ii].PhotoCollection;
                        }

                        if (pc != null && (bChecked || bGetSelected))
                        {
                            int photoNdx = 0;
                            foreach (Photo ph in pc)
                            {
                                bool bSelected = false;
                                if (bGetSelected)
                                {
                                    bSelected = (String.Compare(GetCheck(String.Format("check{0:d5}_{1}", ii, photoNdx)), "1") == 0);
                                }

                                if (bChecked || bSelected)
                                {
                                    _curDLPhotos.Add(new DLPhoto(ph, title));
                                }

                                photoNdx++;
                            }
                        }
                    }
                }

                AddToHistory(ref _destHistory, _destCtrlName, destFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

            if (_curDLPhotos.Count > 0)
            {
                _statusForm = new StatusForm(_bgw, _curDLPhotos);
                Cursor = Cursors.WaitCursor;
                this.Enabled = false;
                _bgw.RunWorkerAsync();
                _statusForm.Show();
            }
            else
            {
                MessageBox.Show(this, "no photos selected");
            }
        }

        public string GetPhotoThumbnails(int ndx)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                Photo[] pc = null;

                if (_browseType == BrowseType.photosets)
                {
                    if (_curPhotosets.PhotosetCollection == null || ndx >= _curPhotosets.PhotosetCollection.Length)
                    {
                        if (_curPhotos == null)
                        {
                            _curPhotos = GetUsersPhotos(_curUser);
                        }
                        pc = _curPhotos.PhotoCollection;
                    }
                    else
                    {
                        Photoset ps = _curPhotosets.PhotosetCollection[ndx];

                        if (ps.PhotoCollection == null)
                        {
                            ps.PhotoCollection = GetPhotosetPhotos(ps);
                        }

                        pc = ps.PhotoCollection;
                    }
                }
                else if (_browseType == BrowseType.tags)
                {
                    pc = _curTagPhotos.PhotoCollection;
                }
                else if (_browseType == BrowseType.groups)
                {
                    if (_curGroupPhotos[ndx] == null)
                    {
                        GroupSearchResult gsr = _curPoolGroups.Groups[ndx];

                        _curGroupPhotos[ndx] = GetGroupPhotos(gsr);
                        pc = _curGroupPhotos[ndx].PhotoCollection;
                    }
                }

                if (pc == null)
                {
                    MessageBox.Show(this, "There are no photos in this set/group");
                }
                else
                {
                    int photoNdx = 0;
                    foreach (Photo ph in pc)
                    {
                        string userId = ph.UserId;
                        if (ph.UserId == null || ph.UserId.Length == 0)
                        {
                            if (_curUser != null)
                            {
                                userId = _curUser.UserId;
                            }
                        }
//                        string pageURL = ph.WebUrl;
                        string pageURL = String.Format("http://flickr.com/photos/{0}/{1}/"
                                                       ,userId
                                                       ,ph.PhotoId
                                                       );
                        sb.Append(
                            String.Format(
                                "<div class=\"thumb1\"><div class=\"thumb2\"><div class=\"thumb3\"><a href=\"{3}\" target=\"_blank\"><img src=\"{0}\" width=\"64\" height=\"64\"/></a><br/><input type=\"checkbox\" onclick=\"checkphoto()\" id=\"check{1:d5}_{2}\"/></div></div></div>"
                                , ph.SquareThumbnailUrl
                                , ndx
                                , photoNdx
                                , pageURL
                                ));
                        photoNdx++;
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            return "";
        }

        public void Authenticate()
        {
            if (_flickrToken.Length > 0)
            {
                _flickrToken = "";
            }
            else
            {
                AuthForm1 authForm = new AuthForm1();

                try
                {
                    string frob = _fapi.AuthGetFrob();
                    string url  = _fapi.AuthCalcUrl(frob, AuthLevel.Read);

                    authForm.label1.Text = "This program requires your authorization before it can read your private photos on Flickr.";
                    authForm.label2.Text = "Authorizing is a simple process which takes place in your web browser. When you're finished, return to this window to complete authorization and begin using Flickrdown.";
                    authForm.label3.Text = "(You must be connected to the internet in order to authorize this program.)";
                    authForm.button1.Text = "Authorize...";

                    if (authForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    authForm.label1.Text = "Return to this window after you have finished the authorization process on Flickr.com";
                    authForm.label2.Text = "Once you're done, click the 'Complete Authorization' button below and you can begin using Flickrdown.";
                    authForm.label3.Text = "(You can revoke this program's authorization at any time in your account page on Flickr.com.)";
                    authForm.button1.Text = "Complete Authorization";

                    System.Diagnostics.Process.Start(url);

                    if (authForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    Auth auth = _fapi.AuthGetToken(frob);
                    _flickrToken = auth.Token;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    _flickrToken = "";
                }
            }
            _fapi.AuthToken = _flickrToken;
            UpdateAuthenticateButton();
        }
    }

    class Util
    {
        static private Dictionary<string, Regex> regexDict = new Dictionary<string, Regex>();

        // save off regexes since making them is expensive
        static public Regex MakeRegex (string pattern, RegexOptions options)
        {
            if (regexDict.ContainsKey(pattern))
            {
                return regexDict[pattern];
            }

            Regex r = new Regex(pattern, options | RegexOptions.Singleline);
            regexDict[pattern] = r;

            return r;
        }

        static public string MakeFilenameSafe(string str)
        {
            //Regex r1 = new Regex("%\\d\\d", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //str = r1.Replace(str, "_");
            //Regex r2 = Util.MakeRegex("[^\\w\\. '+=\\[\\]@,]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex r3 = Util.MakeRegex("[/\\\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			str = r3.Replace(str, "-");
			Regex r4 = Util.MakeRegex("[\\:]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			str = r4.Replace(str, ";");
			Regex r2 = Util.MakeRegex("[/\\\\:\\*\\?\"\\<\\>\\|]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            str = r2.Replace(str, "_");

            return str;
        }

		static public string MakeFilenameSuperSafe(string str)
		{
			//Regex r1 = new Regex("%\\d\\d", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			//str = r1.Replace(str, "_");
			Regex r3 = Util.MakeRegex("[/\\\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			str = r3.Replace(str, "-");
			Regex r4 = Util.MakeRegex("[\\:]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			str = r4.Replace(str, ";");
			Regex r2 = Util.MakeRegex("[^\\w\\. '+=\\[\\]@,]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			str = r2.Replace(str, "_");

			return str;
		}

        static public bool FindLocalFile (ref string filename)
        {
            string work = filename;
            {
                for (int ii = 0; ii < 3; ++ii)
                {
                    if (File.Exists(work))
                    {
                        filename = Path.GetFullPath(work);
                        return true;
                    }
                    work = "../" + work;
                }

                // make sure we are checking the app path
                work = Path.Combine(Application.StartupPath, filename);
                if (File.Exists(work))
                {
                    filename = Path.GetFullPath(work);
                    return true;
                }
            }
            return false;
        }

        static public string GetVersion ()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo FVI = FileVersionInfo.GetVersionInfo(assembly.Location);

            return String.Format("{0}.{1}.{2}", FVI.FileMajorPart, FVI.FileMinorPart, FVI.FileBuildPart, FVI.FilePrivatePart);
        }
    }

    public struct DLPhoto
    {
        public Photo    photo;
        public string   photoSetName;

        public DLPhoto (Photo ph, string setName)
        {
            photo = ph;
            photoSetName = setName;
        }

        public override string ToString()
        {
            return photoSetName + " : " + photo.Title;
        }
    }
}