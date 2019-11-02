using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SQLIteDBConnection dbConnection = null;

        private DataTable companyList = null;
        private bool isNewEntry = false;

        // Pre-defined PIN; use whatever you want
        private int pin = 1234;

        public MainWindow()
        {
            InitializeComponent();

            Setup();

        }

        // Event listener for text typed into combo box
        // Every string is searched for in our database.
        // If a match is found, contents are displayed on the fields. 
        private void cbCompanyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataRow[] result = null;

            var searchTerm = cbCompanyName.Text.Trim();

            if (companyList != null)
            {
                result = companyList.Select("name = '" + searchTerm + "'");
            }
            else
            {
                result = new DataRow[0];
            }

            // Depending on whether company exists in database - toggle visibility for "Decrypt" button.
            if (result.Length == 0)
            {
                // Result = 0 implies a new entry
                btnAddUpdateCompany.Content = "Add";
                btnAddUpdateCompany.Visibility = System.Windows.Visibility.Visible;
                isNewEntry = true;
                btnDecrypt.Visibility = System.Windows.Visibility.Hidden;

            }
            else
            {
                // existing entry is found
                btnAddUpdateCompany.Content = "Update";
                btnAddUpdateCompany.Visibility = System.Windows.Visibility.Visible;
                isNewEntry = false;
                btnDecrypt.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // btnAddUpdateCompany_Click - handles the click event for the btnAddUpdateCompany_Click button.
        // If the company is not in the database, it is a new entry and inserts details into database, otherwise the company details in the database are updated.
        private void btnAddUpdateCompany_Click(object sender, RoutedEventArgs e)
        {
 
            if (FormValidation())
            {
                var encryptedPassword = Security.Encrypt(txtPassword.Text.Trim());

                DataRowView selectedRowView = (DataRowView) cbCompanyName.SelectedValue;

                if (isNewEntry) 
                {
                    var company = new Company(cbCompanyName.Text.Trim(), txtLogin.Text.Trim(), encryptedPassword);
                    dbConnection.InsertNewCompany(company);
                    LoadCompanyDetails();
                    MessageBox.Show("New company registered!");
                    //CleanUI();
                }
                else
                // Existing entry 
                {
                    var company = new Company(Convert.ToInt32(selectedRowView[0]), cbCompanyName.Text.Trim(), txtLogin.Text.Trim(), txtPassword.Text.Trim());
                    
                    // Check the password in the text field to see if it already exists in the database.
                    // If it is, password is already encrypted, don't do anything as the user has not entered a new password.
                    if(dbConnection.PasswordExists(company))
                    {
                        MessageBox.Show("Password already exists in the database, try another one.");
                        return;
                    }
                                       
                    //  Prompt user for PIN in order to update the database.
                    if (PromptForPIN())
                    {
                        // Password entered by the user needs to be encrypted before we write to the database
                        company.Password = Security.Encrypt(txtPassword.Text.Trim());
                        dbConnection.UpdateCompany(company);

                        // Refresh datatable with updated data from database
                        LoadCompanyDetails();
                        MessageBox.Show("Company Updated!");
                        //CleanUI();
                    }
                    else
                    {
                        MessageBox.Show("Wrong PIN");
                    }
                }
            }
        }

        // btnDecrypt_Click - handles the "Decrypt" button event.
        // Checks password field for emptiness. If not empty, PromptsforPIN() is called and if it returns true, it display decrypted password to user.
        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("Password field is empty, can't decrypt");
            }
            else
            {
                if (PromptForPIN())
                {
                    PasswordWindow passwordWindow = new PasswordWindow(Security.Decrypt(txtPassword.Text.Trim()));
                    passwordWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("PIN verification failed");
                }
            }
        }

        // cbCompanyName_SelectionChanged - handles the SelectionChanged event. If the CompanyName combo box is filled in with an entry
        // from the database, it will update the GUI accordingly.       
         private void cbCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCompanyName.SelectedIndex < 0 && cbCompanyName.Text.Trim().Length == 0)
            {
                // No company from the dropdown is selected, hide the Update/New button and the decrypt button
                btnAddUpdateCompany.Visibility = System.Windows.Visibility.Hidden;
                btnDecrypt.Visibility = System.Windows.Visibility.Hidden;
            }

            if (cbCompanyName.SelectedIndex < 0 && cbCompanyName.Text.Trim().Length > 0)
            {
                // No company from the dropdown is selected, hide the Update/New button and the decrypt button
                btnAddUpdateCompany.Visibility = System.Windows.Visibility.Visible;
                btnAddUpdateCompany.Content = "Add";
                isNewEntry = true;
                btnDecrypt.Visibility = System.Windows.Visibility.Hidden;
            }

            else
            {
                // if company from dropdown selected, display "New/Update" button and change its text to "Update"
                // Also show "Decrypt" button
                btnAddUpdateCompany.Visibility = System.Windows.Visibility.Visible;
                btnAddUpdateCompany.Content = "Update";
                isNewEntry = false;
                btnDecrypt.Visibility = System.Windows.Visibility.Visible;
            }
        }
        

        private void AddCompanyItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Enter new userid/password in corresponding fields. If something is the same, just re-enter it");
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        // PromptForPIN() - prompts the user for the security PIN and checks it agaisnt established PIN.
        // Returns true if they match.
        private bool PromptForPIN()
        {
            var pinCheckOutcome = false;
            var promptResponse = 0;
            var userInput = Microsoft.VisualBasic.Interaction.InputBox("Enter PIN", "PIN", "0", -1, -1);

            if (int.TryParse(userInput, out promptResponse))
            {
                if (promptResponse == pin)
                {
                    pinCheckOutcome = true;
                }
            }

            return pinCheckOutcome;
        }

        // PromptForNewPassword() - prompts the user for  a new password for the selected company entry.
        private string PromptForNewPassword()
        {
            var userInput = Microsoft.VisualBasic.Interaction.InputBox("Enter new password", "New Password", "", -1, -1);

            if (userInput.Length == 0)
            {
                MessageBox.Show("New password is not long enough");
            }

            return userInput;
        }

        // Setup() Checks to see if the database exists, if it isn't, i's created
        // If database does exist, LoadCompanyDetails() is called which fills a datatable to be used by the progarm.
        private void Setup()
        {
            dbConnection = new SQLIteDBConnection();

            if (dbConnection.CompanyTableExists())
            {
                LoadCompanyDetails();
            }
            else
            {
                dbConnection.CreateRecordsTable();
                MessageBox.Show("Database did not exist, so it's created and it is empty");
            }
        }

        private void LoadCompanyDetails()
        {
            companyList = dbConnection.RetrieveCompanyDetails();

            if (companyList != null)
            {
                cbCompanyName.ItemsSource = companyList.DefaultView;
            }
        }

        // Cleans the text fields - used after inser/update operation
        private void CleanUI()
        {
            txtLogin.Text = String.Empty;
            txtPassword.Text = String.Empty;
        }

        // Validates form fields on MainWindow
        private bool FormValidation()
        {
            if (cbCompanyName.Text.Trim().Length == 0 || txtLogin.Text.Trim().Length == 0
              || txtPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter Company name, login and/or password.");
                return false;
            }

            if(cbCompanyName.Text.Trim().Length > 25)
            {
                MessageBox.Show("Company Name is too long.  Make it less than 25 characters.");
                return false;
            }

            if (txtLogin.Text.Trim().Length > 25)
            {
                MessageBox.Show("Login is too long. Make it less than 25 characters.");
                return false;
            }

            if (txtPassword.Text.Trim().Length > 25)
            {
                MessageBox.Show("Password is too long. Make it less than 25 characters.");
                return false;
            }

            if (txtPassword.Text.Trim().Length < 7)
            {
                MessageBox.Show("Password is too short. Make it more than 7 characters.");
                return false;
            }

            return true;

        }

    }
}