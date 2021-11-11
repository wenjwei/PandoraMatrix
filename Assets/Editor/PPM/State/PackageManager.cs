using cPackage.Package;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace PPM
{
    public class PackageManager
    {
        private InstalledViewDataObject _installedView;
        private NetworkViewDataObject _networkView;
        private UpdateViewDataObject _updateView;
        private OwnViewDataObject _ownView;

        private PackageMenuData _packageMenuData;
        private PackageSelection _packageSelection;
        private static PackageManager _packageManager;

        public static PackageManager Instance
        {
            get
            {
                if (_packageManager == null)
                {
                    _packageManager = new PackageManager();
                }
                return _packageManager;
            }

            set
            {
                if (value == null)
                {
                    _packageManager.Dispose();
                }
                _packageManager = value;
            }
        }

        public PackageManager()
        {
            _installedView = new InstalledViewDataObject();
            _networkView = new NetworkViewDataObject();
            _updateView = new UpdateViewDataObject();
            _ownView = new OwnViewDataObject();

            _packageMenuData = new PackageMenuData();
            _packageSelection = new PackageSelection();

            PPMUtils.LogHelper.Init(PPMHelper.Log, PPMHelper.LogWarning, PPMHelper.LogError);
        }

        public event Action OnMenuViewRefreshed;
        public event Action OnListViewRefreshed;
        public event Action OnDetailViewRefreshed;
        public event Action OnPackageUploadWindowRefreshed;
        public delegate void OnResponse(int ret, string massage);

        private bool _initiated;

        public bool InitiateEnvironment()
        {
            if (_initiated)
            {
                return true;
            }

			SetServerPath(PPMHelper.GetServerAddress());
            PPMUtils.LibPackageInterface.Init();
            return _initiated = true;
        }

        public void GetInstalledPackages()
        {
            _installedView.RequestData(ViewSortType.DESC, "", 1, "", OnMenuViewRefreshed);
        }

        #region ListView

        public List<PPMPackageInfo> GetPackageListViewListData()
        {
            PackageMenuItem currentMenuItem = _packageSelection.CurrentMenuItem;
            switch (currentMenuItem.MenuItemEnum)
            {
                case EPackageMenuItem.Installed:
                    return _installedView.GetViewData();
                case EPackageMenuItem.Network:
                    return _networkView.GetViewData();
                case EPackageMenuItem.Update:
                    return _updateView.GetViewData();
                case EPackageMenuItem.Own:
                    return _ownView.GetViewData();
                default:
                    return _networkView.GetViewData();
            }
        }

        public int GetTotalPackageCount()
        {
            PackageMenuItem currentMenuItem = _packageSelection.CurrentMenuItem;
            switch (currentMenuItem.MenuItemEnum)
            {
                case EPackageMenuItem.Installed:
                    return _installedView.GetTotalPackageCount();
                case EPackageMenuItem.Network:
                    return _networkView.GetTotalPackageCount();
                case EPackageMenuItem.Update:
                    return _updateView.GetTotalPackageCount();
                case EPackageMenuItem.Own:
                    return _ownView.GetTotalPackageCount();
                default:
                    return _networkView.GetTotalPackageCount();
            }
        }

        public int GetTotalPageCount()
        {
            PackageMenuItem currentMenuItem = _packageSelection.CurrentMenuItem;
            switch (currentMenuItem.MenuItemEnum)
            {
                case EPackageMenuItem.Installed:
                    return _installedView.GetTotalPageCount();
                case EPackageMenuItem.Network:
                    return _networkView.GetTotalPageCount();
                case EPackageMenuItem.Update:
                    return _updateView.GetTotalPageCount();
                case EPackageMenuItem.Own:
                    return _ownView.GetTotalPageCount();
                default:
                    return _networkView.GetTotalPageCount();
            }
        }

        public void RequestData(int page)
        {
            PackageMenuItem currentMenuItem = _packageSelection.CurrentMenuItem;
            ViewSortType sortType = ViewSortType.ASC;
            if (currentMenuItem.CurrentSortInfo != null)
                sortType = currentMenuItem.CurrentSortInfo.SortType;

            string tag = currentMenuItem.CurrentTag;
            string search = _packageSelection.CurrentSearchStr;

            switch (currentMenuItem.MenuItemEnum)
            {
                case EPackageMenuItem.Installed:
                    _installedView.RequestData(sortType, tag, page, search, OnListViewRefreshed);
                    break;
                case EPackageMenuItem.Network:
                    _networkView.RequestData(sortType, tag, page, search, OnListViewRefreshed);
                    break;
                case EPackageMenuItem.Update:
                    _updateView.RequestData(sortType, null, page, search, OnListViewRefreshed);
                    break;
                case EPackageMenuItem.Own:
                    _ownView.RequestData(sortType, tag, page, search, OnListViewRefreshed);
                    break;
            }
        }

        public void RequestNeedUpdateCount()
        {
            string menuName = _updateView.GetTotalPackageCount() == 0 ?
                "更新" : string.Format("更新({0})", _updateView.GetTotalPackageCount());

            Action refreshUpdateCountCallback = () =>
            {
                MenuData md = _menuTree.FindAsset(menuName);
                md.menuName = _updateView.GetTotalPackageCount() == 0 ?
                    "更新" : string.Format("更新({0})", _updateView.GetTotalPackageCount());
            };

            _updateView.RequestData(ViewSortType.ASC, "", 0, "", refreshUpdateCountCallback);
        }

        private List<PPMPackageInfo> FilterListData(List<PPMPackageInfo> list, string filterStr)
        {
            if (string.IsNullOrEmpty(filterStr))
            {
                return list;
            }

            List<PPMPackageInfo> filterList = new List<PPMPackageInfo>();
            foreach (var packageInfo in list)
            {
                if (packageInfo.name.ToLower().Contains(filterStr.ToLower()))
                {
                    filterList.Add(packageInfo);
                }
            }
            return filterList;
        }

        public int GetCurrentSortSelectedIndex()
        {
            return _packageSelection.CurrentSortSelectedIndex;
        }

        public void SetCurrentSortSelectedIndex(int currentIndex)
        {
            _packageSelection.SetCurrentSortSelectedIndex(currentIndex);
            switch (_packageSelection.CurrentMenuItem.MenuItemEnum)
            {
                case EPackageMenuItem.Network:
                    _networkView.ChangeSortType(_packageSelection.CurrentMenuItem.SortInfoList[currentIndex].SortType);
                    break;
            }
            OnListViewRefreshed();
        }

        public List<string> GetSortOptionList()
        {
            return _packageSelection.CurrentMenuItem != null ?
                _packageSelection.CurrentMenuItem.GetSortOptionList() : null;
        }

        public void SetCurrentPackageViewItem(PPMPackageInfo packageViewItem)
        {
			Action<PPMPackageInfo> action = (packageInfo) =>
			{
				_packageSelection.CurrentPackageViewItem = packageInfo;
				OnDetailViewRefreshed();
				PPMHelper.RefreshWindow<PackageManagerWindow>();
			};

			if (_packageSelection.CurrentMenuItem.MenuItemEnum == EPackageMenuItem.Network)
			{
				_networkView.RequestPackageDetail(packageViewItem, action);
			}
			action(packageViewItem);
		}

        public PPMPackageInfo GetCurrentPackageViewItem()
        {
            return _packageSelection.CurrentPackageViewItem;
        }

        #endregion

        #region MenuView

        public void SelectMenu(MenuData menuData)
        {
            _packageSelection.SelectMenu(_packageMenuData.MenuItemList, menuData);
            RequestData(1);
        }

        private MenuTree _menuTree;

        public MenuTree GetMenuTree()
        {
            List<string> menuContentList = new List<string>();
            foreach (var menuItem in _packageMenuData.MenuItemList)
            {
                if (menuItem.MenuItemEnum == EPackageMenuItem.Update)
                {
                    string menuContent = menuItem.GetMenuList()[0];
                    menuContentList.Add(menuContent);
                }
                else
                {
                    menuContentList.AddRange(menuItem.GetMenuList());
                }
            }

            _menuTree = new MenuTree();
            foreach (var menuContent in menuContentList)
                _menuTree.AddAsset(menuContent);
            return _menuTree;
        }

        #endregion

        #region DetailView

        public void SetCurrentSearchStr(string searchStr)
        {
            _packageSelection.CurrentSearchStr = searchStr.Trim();
            _installedView.changeSearchStr(searchStr);
            OnListViewRefreshed();
        }

        public PackageMenuItem GetCurrentMenuItem()
        {
            return _packageSelection.CurrentMenuItem;
        }

        #endregion

        #region UserState

        public string UserName = "";

        public void SetLoginCallback(Action<string> loginCallback)
        {
            Action<string> callback = (userName) =>
            {
                UserName = userName;
                if (loginCallback != null)
                    loginCallback(UserName);
            };
            PPMUtils.Requestor.LoginCallback = callback;
        }

        public void UnsetLoginCallback()
        {
            PPMUtils.Requestor.LoginCallback = null;
        }

        public void GetCookieUID(Action<string> getCallback)
        {
            PPMUtils.Requestor.GetCookieUID(getCallback);
        }

        #endregion

        #region PackageManger

        public void PackagePublish(string packageName, string version, int state, PPMPackageInfo ppmPackageInfo)
        {
            Action<PPMUtils.ResponseStatus> requestCallback = (data) =>
            {
                Action<int> callback = (ret) =>
                {
                    foreach (var release in ppmPackageInfo.releases)
                    {
                        if (release.version.Equals(version))
                        {
                            release.publishState = state;
                            break;
                        }
                    }
                    SetCurrentPackageViewItem(ppmPackageInfo);
                    PPMHelper.RefreshWindow<PackageManagerWindow>();
                };
                ResponseCheck(data, "PublishPackage", callback);
            };
            PPMUtils.Requestor.PackagePublish(packageName, version, state, requestCallback);
        }

        public void PackageUpload(string packageName, string packageType, string version, string tag, string description, string dependence, string uploadFile,
            string changeLog)
        {
            Action<PPMUtils.ResponseStatus> requestCallback = (data) =>
            {
                Action<int> callback = (start) =>
                {
                    OnPackageUploadWindowRefreshed();
                };
                ResponseCheck(data, "UploadPackage", callback);
            };
            PPMUtils.Requestor.PackageUpload(packageName, packageType, version, tag, description,
                dependence, uploadFile, changeLog, requestCallback);
        }

        public void PackageDelete(string packageName, string packageType, string version, string location)
        {
            Action<PPMUtils.ResponseStatus> requestCallback = (data) =>
            {
                Action<int> callback = (start) =>
                {
                    if (OnListViewRefreshed != null)
                        OnListViewRefreshed();
                };
                ResponseCheck(data, "DeletePackage", callback);
            };
            PPMUtils.Requestor.PackageDelete(packageName, packageType, version, location, requestCallback);
        }

        public void PackageDownload(string packageName, string packageVersion, string savePath, Action<float> requestProgress, Action<string> callback)
        {
            Action<PPMUtils.ResponseStatus> requestCallback = (data) =>
            {
                Action<int> callBack = (start) =>
                {
                    callback(data.msg);
                };
                ResponseCheck(data, "DownloadPackage", callBack);
            };
            PPMUtils.Requestor.PackageDownload(packageName, "Unity", packageVersion, requestCallback);
        }

        public void ImportPackage(string localPackagePath)
        {
            if (!localPackagePath.EndsWith(".ppm"))
            {
                PPMHelper.LogWarning(string.Format("导入插件失败，{0} 为不支持的导出类型！", localPackagePath));
                return;
            }

            string cpackagePath = "";
            PPMConfiguration conf = PPMHelper.DecompressPPM(localPackagePath, ref cpackagePath);

            Action<Boolean> importCallback = (ret) =>
            {
                PPMHelper.DeletePPMDecompressDir();

                if (!ret)
                {
                    PPMHelper.Log(string.Format("用户取消导入插件: {0}", conf.PackageName));
                    return;
                }

                PPMHelper.InstallPackage(conf);
            };
            cPackageExporter.ImportCPackage(cpackagePath, importCallback);
        }

        #endregion

        #region Server
        public void SetServerPath(string server)
        {
            Action<string> setServerCallBack = (ret) =>
            {
                if (!string.IsNullOrEmpty(ret))
                {
                    PPMHelper.Log(string.Format("设置后台地址失败: {0}", server));
                    return;
                }
                else
                {
                    PPMHelper.Log(string.Format("设置后台地址成功: {0}", server));
                    return;
                }
            };
            PPMUtils.Requestor.SetServerPath(server, setServerCallBack);
        }

        public void GetServerPath()
        {
            Action<PPMUtils.ServerInfo> getServerCallBack = (ret) =>
            {
                PPMHelper.Log(string.Format("获取后台地址成功: {0}", ret.server));
                return;
            };
            PPMUtils.Requestor.GetServerPath(getServerCallBack);
        }
        #endregion


        private void ResponseCheck(PPMUtils.ResponseStatus response, string content, Action<int> callback)
        {
            if (response.ret != 0)
            {
                PPMHelper.LogWarning(content + " warning :" + response.msg);
            }
            else
            {
                callback(0);
            }
        }

        private void Dispose()
        {
            PPMUtils.LogHelper.Dispose();
        }

        private void FileDelete(string path)
        {
            System.Action action = () =>
            {
                try { FileUtil.DeleteFileOrDirectory(path); }
                catch (System.Exception e)
                {
                    PPMHelper.LogWarning("Delete file exception" + e + ",path :" + path);
                }
            };
            ThreadUtils.ExecuteOnMainThread(action);
        }
    }
}
