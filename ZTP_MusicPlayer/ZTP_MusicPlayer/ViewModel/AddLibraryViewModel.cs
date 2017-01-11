using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;
using ZTP_MusicPlayer.View;

namespace ZTP_MusicPlayer.ViewModel
{
    internal class AddLibraryViewModel : INotifyPropertyChanged
    {
        private readonly MediaPlayer player = MediaPlayer.Instance;
        #region Members

        private ICommand addLibraryCommand;
        private ICommand removeLibraryCommand;

        #endregion
        #region Properties

        public Library SelectedLibrary { get; set; }

        public ObservableCollection<Library> Libraries
        {
            get { return player.Libraries; }
        }

        public ICommand AddLibraryCommand
        {
            get
            {
                if (addLibraryCommand == null)
                {
                    addLibraryCommand = new RelayCommand(AddLibraryExecute, AddLibraryCanExecute);
                }
                return addLibraryCommand;
            }
            set { addLibraryCommand = value; }
        }


        public ICommand RemoveLibraryCommand
        {
            get
            {
                if (removeLibraryCommand == null)
                {
                    removeLibraryCommand = new RelayCommand(RemoveLibraryExecute, RemoveLibraryCanExecute);
                }
                return removeLibraryCommand;
            }
            set { removeLibraryCommand = value; }
        }

        #endregion
        #region Commands(Can)Execute
        private bool AddLibraryCanExecute(object o)
        {
            return true;
        }

        private void AddLibraryExecute(object obj)
        {
            var dialog = new CreateNewLibraryWindow();
            dialog.ShowDialog();
        }

        private bool RemoveLibraryCanExecute(object o)
        {
            return SelectedLibrary != null;
        }

        private void RemoveLibraryExecute(object obj)
        {
            player.RemoveLibrary(SelectedLibrary.Name);                               
        }
        #endregion
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion       
    }
}