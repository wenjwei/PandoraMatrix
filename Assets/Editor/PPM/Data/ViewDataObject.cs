using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

using PPMUtils;
using Newtonsoft.Json;
using Semver;

namespace PPM
{
	interface IViewDataObject
	{
		void RequestData(ViewSortType sortType, string tag, int pageIndex, string searchStr, Action callback);
		List<PPMPackageInfo> GetViewData();
		int GetTotalPackageCount();
		int GetTotalPageCount();
	}

	public class ViewData
	{
		public const int OnePageCount = 10;
	}

	public enum ViewSortType
	{
		DESC,	// 降序
		ASC,	// 升序
	}

	public class NetworkViewDataObject : ViewData, IViewDataObject
	{		
        private int _totalPackageCount;
        private int _currentPageIndex = 1;
        private string _currentSearchStr;
        private string _currentPackageTag;
        private ViewSortType _currentSortType;

        private List<PPMPackageInfo> _viewDataList;

		public NetworkViewDataObject()
		{
			_viewDataList = new List<PPMPackageInfo>();
		}

		public void RequestData(ViewSortType sortType, string tag, int pageIndex, string searchStr, Action callback)
		{
			FakeProgressBar.DisplayFakeProgressBar("插件管理器", "获取插件列表");

            _currentPackageTag = tag;
            _currentSortType = sortType;
			_currentPageIndex = pageIndex;
            _currentSearchStr = searchStr;

			Action<DataPackageList> requestCallback = (data) =>
			{
				ParseData(data);
				if (callback != null)
				{
					callback();
				}
				FakeProgressBar.ClearFakeProgressBar();
			};

			Requestor.GetPackageList(_currentSortType.ToString(), _currentPackageTag, _currentPageIndex, 
				OnePageCount, requestCallback);
		}

        public void ChangeSortType(ViewSortType viewSortType)
        {
            _currentSortType = viewSortType;
        }

		private void ParseData(DataPackageList data)
		{
			ResetData();

			if (data == null || data.packages == null)
            {
                return;
            }

			_totalPackageCount = data.totalNum;
			foreach(var package in data.packages)
			{
                PPMPackageInfo ppmPackage = new PPMPackageInfo()
                {
                    name = package.name,
                    description = package.description,
                    author = package.author,
                    createTime = package.createTime,
					labels = package.tag == null ? "" : string.Join(";", package.tag.ToArray()),
					score = package.score.ToString() == "0" ? "暂无" : package.score.ToString(),
					releases = new List<ReleaseInfo>()
				};

				ReleaseInfo ri = new ReleaseInfo()
				{
					version = package.version,
					url = package.url,
					date = ""
				};
				ppmPackage.releases.Add(ri);
				_viewDataList.Add(ppmPackage);
			}
		}

		public void RequestPackageDetail(PPMPackageInfo packageViewItem, Action<PPMPackageInfo> callback)
		{
			Action<DataPackageDetail> requestCallback = (packageDetail) =>
			{
				packageDetail.packages.Sort((i, j) => SemVersion.Parse(i.version).CompareTo(SemVersion.Parse(j.version)));

				packageViewItem.releases.Clear();
				foreach (var package in packageDetail.packages)
				{
					ReleaseInfo ri = new ReleaseInfo()
					{
						version = package.version,
						changeLog = package.changeLog
					};
					packageViewItem.releases.Add(ri);
				}

				if (callback != null)
					callback(packageViewItem);
			};
			Requestor.GetPackageDetail(packageViewItem.name, requestCallback);
		}

		public List<PPMPackageInfo> GetViewData()
		{
            RequestData(_currentSortType,_currentPackageTag,_currentPageIndex,_currentSearchStr,null);

            return _viewDataList;
		}

		public int GetTotalPackageCount()
		{
			return _totalPackageCount;
		}

		public int GetTotalPageCount()
		{
			return Mathf.CeilToInt(_totalPackageCount / (float)OnePageCount);
		}

		private void ResetData()
		{
			_viewDataList.Clear();
			_totalPackageCount = 0;
		}
	}

	public class InstalledViewDataObject : ViewData, IViewDataObject
	{
        private int _totalPackageCount;
        private int _currentPageIndex = 1;
        private string _currentPackageTag;
        private string _currentSearchStr;
        private ViewSortType _currentSortType;
        private List<PPMPackageInfo> _viewDataList = new List<PPMPackageInfo>();

        public void RequestData(ViewSortType sortType, string tag, int pageIndex, string searchStr, Action callback)
		{
            _currentSortType = sortType;
            _currentPackageTag = tag;
            _currentPageIndex = pageIndex;
            _currentSearchStr = searchStr;

			string config = PPMHelper.ReadStringFromFile(PPMHelper.GetInstalledConfigurationPath());
			ParseData(config, sortType, tag, pageIndex, searchStr);

			if (callback != null)
				callback();
        }

		private void ParseData(string config, ViewSortType sortType,string tag,int pageIndex,string searchStr)
        {
			if (string.IsNullOrEmpty(config))
            {
                return;
            }

            ResetData();
            List<PPMConfiguration> data = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);

            foreach (var package in data)
            {
                PPMPackageInfo ppmPackage = new PPMPackageInfo()
                {
                    name = package.PackageName,
                    description = package.PackageDescription,
                    author = package.PackageAuthor,
                    labels = package.PackageTags == null ? "" : package.PackageTags,
                    releases = new List<ReleaseInfo>(),
                };

                ReleaseInfo ri = new ReleaseInfo()
                {
                    version = package.PackageVersion,
                    url = "",
                    date = ""
                };

                ppmPackage.releases.Add(ri);

                if ((ppmPackage.name.ToLower().Contains(searchStr == null ? "":searchStr.ToLower())||
                     ppmPackage.description.ToLower().Contains(searchStr == null ? "" : searchStr.ToLower())) &&
                     ppmPackage.labels.Contains(tag ?? ""))
                {
                    _totalPackageCount++;
                    _viewDataList.Add(ppmPackage);
                }
            }
        }

        private void ResetData()
        {
            _viewDataList.Clear();
            _totalPackageCount = 0;
        }

        public void changeSearchStr(string searchStr)
        {
            _currentSearchStr = searchStr;
        }

        public List<PPMPackageInfo> GetViewData()
		{
            RequestData(_currentSortType, _currentPackageTag, _currentPageIndex, _currentSearchStr, null);
            return _viewDataList;
		}

		public int GetTotalPackageCount()
		{
			return _totalPackageCount;
		}

		public int GetTotalPageCount()
		{
			return Mathf.CeilToInt(_totalPackageCount / (float)OnePageCount);
		}
	}

	public class UpdateViewDataObject : ViewData, IViewDataObject
	{
        private int _totalPackageCount =0;
        private int _currentPageIndex = 1;
        private string _currentSearchStr;
        private string _currentPackageTag;
        private ViewSortType _currentSortType;
        private List<PPMPackageInfo> _viewDataList = new List<PPMPackageInfo>();
		private bool _init;

		public UpdateViewDataObject()
		{
			_init = false;
		}

        public void RequestData(ViewSortType sortType, string tag, int pageIndex, string searchStr, Action callback)
		{
			if (_init)
			{
				if (callback != null)
					callback();
				return;
			}

			_init = true;
			_currentSortType = sortType;
			_currentPackageTag = tag;
			_currentPageIndex = pageIndex;
			_currentSearchStr = searchStr;

			string config = PPMHelper.ReadStringFromFile(PPMHelper.GetInstalledConfigurationPath());
			ParseData(config, searchStr, callback);
		}

        private void ParseData(string config, string searchStr, Action callback)
        {
			if (string.IsNullOrEmpty(config))
			{
				return;
			}

            ResetData();

            List<PPMConfiguration> configurationList = JsonConvert.DeserializeObject<List<PPMConfiguration>>(config);
			int updateTaskCount = 0;
            foreach (var conf in configurationList)
            {
				if (conf.DisableUpdateNotify)
					continue;

				updateTaskCount++;

				PPMPackageInfo installedPackage = new PPMPackageInfo()
                {
                    name = conf.PackageName,
                    description = conf.PackageDescription,
                    author = conf.PackageAuthor,
                    labels = conf.PackageTags ?? "",
                    releases = new List<ReleaseInfo>(),
                };
                ReleaseInfo ri = new ReleaseInfo()
                {
                    version = conf.PackageVersion,
                    url = "",
                    date = ""
                };
				installedPackage.releases.Add(ri);

                Action<DataPackageDetail> requestCallback = (packageDetail) =>
				{
					updateTaskCount--;
					string newVersion = "0.0.0";
					bool needUpdate = IsNeedUpdate(packageDetail, installedPackage, ref newVersion);
					if (needUpdate)
					{
						searchStr = (searchStr ?? "").ToLower();
						if (installedPackage.name.ToLower().Contains(searchStr)
						|| installedPackage.description.ToLower().Contains(searchStr))
						{
							ReleaseInfo newRi = new ReleaseInfo()
							{
								version = newVersion,
								url = "",
								date = ""
							};
							installedPackage.releases.Add(newRi);

							_totalPackageCount++;
							_viewDataList.Add(installedPackage);
						}
					}

					if (updateTaskCount == 0)
					{
						if (callback != null)
							callback();
					}
				};
                Requestor.GetPackageDetail(conf.PackageName, requestCallback);
            }

			if (updateTaskCount == 0)
			{
				if (callback != null)
					callback();
			}
		}

        public bool IsNeedUpdate(DataPackageDetail netPackage, PPMPackageInfo localPackage, ref string newVersion)
        {
			if (netPackage == null || netPackage.packages == null)
				return false;

			netPackage.packages.Sort((i, j) => SemVersion.Parse(j.version).CompareTo(SemVersion.Parse(i.version)));
			newVersion = netPackage.packages[0].version;
			SemVersion netVersion = SemVersion.Parse(netPackage.packages[0].version);
			SemVersion localVersion = SemVersion.Parse(localPackage.releases[0].version);
			return netVersion > localVersion;
        }

        private void ResetData()
        {
            _viewDataList.Clear();
            _totalPackageCount = 0;
        }

        public List<PPMPackageInfo> GetViewData()
		{
            RequestData(_currentSortType, _currentPackageTag, _currentPageIndex, _currentSearchStr, null);
            return _viewDataList;
		}

		public int GetTotalPackageCount()
		{
			return _totalPackageCount;
		}

		public int GetTotalPageCount()
		{
			return Mathf.CeilToInt(_totalPackageCount / (float)OnePageCount);
		}
	}

    public class OwnViewDataObject : ViewData, IViewDataObject
	{
        private int _totalPackageCount;
        private int _currentPageIndex = 1;
        private string _currentSearchStr;
        private string _currentPackageTag;
        private ViewSortType _currentSortType;
        private List<PPMPackageInfo> _viewDataList = new List<PPMPackageInfo>();

        public void RequestData(ViewSortType sortType, string tag, int pageIndex, string searchStr, Action callback)
        {
            FakeProgressBar.DisplayFakeProgressBar("插件管理器", "获取插件列表");

            _currentSortType = sortType;
            _currentPackageTag = tag;
            _currentPageIndex = pageIndex;
            _currentSearchStr = searchStr;

            Action<DataOwnPackages> requestCallback = (data) =>
            {
                ParseData(data,searchStr);
                if (callback != null)
                {
                    callback();
                }
                FakeProgressBar.ClearFakeProgressBar();
            };

            Requestor.GetOwnPackages("Unity", requestCallback);
        }

        private void ParseData(DataOwnPackages data,string searchStr)
        {
            ResetData();

            if (data == null || data.plugins == null)
            {
                return;
            }
            foreach (var package in data.plugins)
            {
				PPMPackageInfo ppmPackage = new PPMPackageInfo()
				{
					name = package.name,
					description = package.description,
					labels = package.tag == null ? "" : string.Join(";", package.tag.ToArray()),
					releases = new List<ReleaseInfo>()
				};

				foreach (var packageItem in package.packages)
				{
					ReleaseInfo ri = new ReleaseInfo()
					{
						version = packageItem.version,
						url = packageItem.url,
						changeLog = packageItem.changeLog,
						publishState = packageItem.publishState
					};

					ppmPackage.releases.Add(ri);
				}

				if (ppmPackage.name.ToLower().Contains(searchStr == null ? "" : searchStr.ToLower()) ||
				   ppmPackage.description.ToLower().Contains(searchStr == null ? "" : searchStr.ToLower()))
				{
					_totalPackageCount++;
					_viewDataList.Add(ppmPackage);
				}
			}
        }

        private void ResetData()
        {
            _viewDataList.Clear();
            _totalPackageCount = 0;
        }

        public List<PPMPackageInfo> GetViewData()
        {
            RequestData(_currentSortType, _currentPackageTag, _currentPageIndex, _currentSearchStr, null);
            return _viewDataList;
        }

        public int GetTotalPackageCount()
        {
            return _totalPackageCount;
        }

		public int GetTotalPageCount()
		{
			return Mathf.CeilToInt(_totalPackageCount / (float)OnePageCount);
		}
	}
}