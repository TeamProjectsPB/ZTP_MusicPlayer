using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZTP_MusicPlayer.Command;
using ZTP_MusicPlayer.Model;
using ZTP_MusicPlayer.View;

namespace ZTP_MusicPlayer.ViewModel
{
    class AddLibraryViewModel : INotifyPropertyChanged
    {
        private MediaPlayer player = MediaPlayer.Instance;



        #region Members
        private Library selectedLibrary;
        private ICommand addLibraryCommand;
        private ICommand removeLibraryCommand;
        #endregion

        #region Properties

        public Library SelectedLibrary
        {
            get { return selectedLibrary; }
            set { selectedLibrary = value; }
        }
       
        public ObservableCollection<Library> Libraries { get { return player.Libraries; } }

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
        private bool AddLibraryCanExecute(object o)
        {
            return true;
        }
        private void AddLibraryExecute(object obj)
        {
            CreateNewLibraryWindow dialog = new CreateNewLibraryWindow();
            dialog.ShowDialog();
        }
        private bool RemoveLibraryCanExecute(object o)
        {
            return selectedLibrary != null;
        }
        private void RemoveLibraryExecute(object obj)
        {
            player.RemoveLibrary(selectedLibrary.Name);
            //player.UpdateSourceTrigger("Libraries");
            //OnPropertyChanged("Libraries");                     
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
