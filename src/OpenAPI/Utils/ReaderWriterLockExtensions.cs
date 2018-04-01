using System;
using System.Threading;

namespace OpenAPI.Utils
{
	internal static class ReaderWriterLockExtensions
	{
		public static ReadLock Read(this ReaderWriterLockSlim rwl)
		{
			return new ReadLock(rwl);
		}

		public static WriteLock Write(this ReaderWriterLockSlim rwl)
		{
			return new WriteLock(rwl);
		}

		/*public static UpgradeableReaderWriterLock UpgreadableReadLock(this ReaderWriterLockSlim rwl)
		{
			return new UpgradeableReaderWriterLock(rwl);
		}*/

		public class ReadLock : IDisposable
		{
			private ReaderWriterLockSlim TheLock { get; }

			internal ReadLock(ReaderWriterLockSlim rwl)
			{
				TheLock = rwl;
				TheLock.EnterReadLock();
			}

			public void Dispose()
			{
				TheLock.ExitReadLock();
			}
		}

		public class WriteLock : IDisposable
		{
			private ReaderWriterLockSlim TheLock { get; }

			internal WriteLock(ReaderWriterLockSlim rwl)
			{
				TheLock = rwl;
				TheLock.EnterWriteLock();
			}

			public void Dispose()
			{
				TheLock.ExitWriteLock();
			}
		}

		/*public class UpgradeableReaderWriterLock : IDisposable
		{
			private ReaderWriterLockSlim TheLock { get; }

			private bool _isInWriteLock = false;
			internal UpgradeableReaderWriterLock(ReaderWriterLockSlim rwl)
			{
				TheLock = rwl;
				TheLock.EnterUpgradeableReadLock();
			}
			

			public void Upgrade()
			{
				if (_isInWriteLock) //We are already upgraded.
				{
					return;
				}
				_isInWriteLock = true;

				TheLock.EnterWriteLock();
			}

			public void ExitWriteLock()
			{
				if (!_isInWriteLock)
				{
					return; //We aren't in a write lock.
				}

				_isInWriteLock = false;
				TheLock.ExitWriteLock();
			}

			public void Dispose()
			{
				if (_isInWriteLock)
				{
					TheLock.ExitWriteLock();
				}
				TheLock.ExitUpgradeableReadLock();
			}
		}*/
	}
}
