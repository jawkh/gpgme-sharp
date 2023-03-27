Preparations (Follow the steps in Sequence)
===========================================

1. Beforehand, please install the GnuPG requirements for the gpgme-sharp NuGet package on your development machine. 

2. Begin by Building the Solution (F6). You should be able to successfully build the solution without any errors.

3. Generate and import 2 sets of OpenPGP Keypairs with same cipher specifications - for the sender (alice) and the recipient (bob) in this sample code. Enable secret passphrase for each of them. 

4. Decide on the method for protecting the confidentiality of the secret passphrase. 
   This solution offers 2 options: 
 
     a. Use AWS Secrets Manager 
     
     b. Use Windows Data Protection API (Only works for Windows OS. Refer to https://simplecodesoftware.com/articles/how-to-encrypt-data-on-macos-without-dpapi for a workaround that provides a similar solution for Linux/macOS systems)
   
   Refer to the respective steps in SETUP.docx to enable each of the option above. 

5. You may run the sample code in Debug Mode / Without Debug Mode (F5 / Ctrl + F5) after you completed the required steps in SETUP.docx. 