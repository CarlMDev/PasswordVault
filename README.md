# PasswordVault
A simple non-networked WPF application that keep track of login credentials

This app uses a SQLite database to store credentials and each password entry is encrypted using 256 bit encryption.

It creates an empty SQLite database file when it is first launched.

To save a set of credentials, input  the details into the "Company Name", "Login" and "Password" fields.

As you start typing, if the company name already exists in the database, the "Update" and "Encrypt" buttons will be visible.

If the company is not found, an "Add" button will be displayed instead.

