import os
import glob

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/Services'

for file_path in glob.glob(os.path.join(test_dir, '*Tests.cs')):
    with open(file_path, 'r') as f:
        content = f.read()
    
    if '.BuildMock()' in content:
        new_content = content.replace('.BuildMock()', '.BuildMockDbSet()')
        
        with open(file_path, 'w') as f:
            f.write(new_content)
            print(f"Fixed {file_path}")

