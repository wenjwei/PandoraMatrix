using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Semver;

namespace PPM
{
	[Serializable]
	public class ReleaseInfo : IComparable<ReleaseInfo>, IComparable
	{
		public string version;
		public string url;
		public string date;
		public string changeLog;
		public int publishState; 

		public int CompareTo(object obj)
		{
			return CompareTo((ReleaseInfo)obj);
		}

		public int CompareTo(ReleaseInfo other)
		{
			if (ReferenceEquals(other, null))
				return 1;

			SemVersion semVersion1, semVersion2;
			bool tryParse1Ret = SemVersion.TryParse(version, out semVersion1);
			bool tryParse2Ret = SemVersion.TryParse(other.version, out semVersion2);
			if (tryParse1Ret && tryParse2Ret)
			{
				return semVersion1.CompareTo(semVersion2);
			}
			else
			{
				throw new ArgumentException("ReleaseInfo.version can not parse");
			}
		}

		public static bool operator ==(ReleaseInfo left, ReleaseInfo right)
		{
			return left.version.Equals(right.version);
		}

		public static bool operator !=(ReleaseInfo left, ReleaseInfo right)
		{
			return !left.version.Equals(right.version);
		}

		public static bool operator >(ReleaseInfo left, ReleaseInfo right)
		{
			return left.version.CompareTo(right.version) > 0;
		}

		public static bool operator >=(ReleaseInfo left, ReleaseInfo right)
		{
			return left == right || left > right;
		}

		public static bool operator <(ReleaseInfo left, ReleaseInfo right)
		{
			return left.version.CompareTo(right.version) < 0;
		}

		public static bool operator <=(ReleaseInfo left, ReleaseInfo right)
		{
			return left == right || left < right;
		}

		public override int GetHashCode()
		{
			return version.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, null))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return this == (ReleaseInfo)obj;
		}
	}

	[Serializable]
	public class PPMPackageInfo
	{
		public string name;
		public string description;
		public string author;
		public string labels;
		public string score;
        public string createTime;
		public List<ReleaseInfo> releases;
    }

	public class PPMConfiguration
	{
		public class ResourceItem
		{
			public string GUID;
			public string Path;
			public string ContentMd5;
			public string MetaMd5;
		}

		[System.NonSerialized]
		public string ToPackPackagePath;
		public string PackageName;
		public string PackageType;
		public string PackageVersion;
		public string PackageAuthor;
		public bool DisableUpdateNotify;
		public bool DisableAutoUpdate;
		public string PackageDescription;
		public string PackageTags;
		public string PackageDependencies;
		public string PackageReleaseNote;
		public List<ResourceItem> ResourceItemList;

		public bool IsDataReady()
		{
			return !string.IsNullOrEmpty(ToPackPackagePath) && !string.IsNullOrEmpty(PackageName)
				&& !string.IsNullOrEmpty(PackageType) && !string.IsNullOrEmpty(PackageVersion);
		}

		public PPMConfiguration()
		{
			ToPackPackagePath = "";
			PackageName = "";
			PackageType = "";
			PackageVersion = "";
			PackageAuthor = "";
			DisableUpdateNotify = false;
			DisableAutoUpdate = false;
			PackageDescription = "";
			PackageTags = "";
			PackageDependencies = "";
			PackageReleaseNote = "";
			ResourceItemList = new List<ResourceItem>();
		}

		public void CopyFrom(PPMConfiguration conf)
		{
			ToPackPackagePath = conf.ToPackPackagePath;
			PackageName = conf.PackageName;
			PackageType = conf.PackageType;
			PackageVersion = conf.PackageVersion;
			PackageAuthor = conf.PackageAuthor;
			DisableUpdateNotify = conf.DisableUpdateNotify;
			DisableAutoUpdate = conf.DisableAutoUpdate;
			PackageDescription = conf.PackageDescription;
			PackageTags = conf.PackageTags;
			PackageDependencies = conf.PackageDependencies;
			PackageReleaseNote = conf.PackageReleaseNote;
			ResourceItemList = new List<ResourceItem>();
			foreach(var item in conf.ResourceItemList)
			{
				ResourceItemList.Add(item);
			}
		}
	}
}
