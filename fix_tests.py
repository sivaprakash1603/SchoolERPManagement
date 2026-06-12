import os
import glob

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/Services'

mapper_setup = """
        var mapperConfig = new AutoMapper.MapperConfiguration(c => 
        {
            c.AddProfile<SchoolERPManagementBLLibrary.Profiles.AppMappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
"""

for file_path in glob.glob(os.path.join(test_dir, '*Tests.cs')):
    with open(file_path, 'r') as f:
        content = f.read()
    
    if 'new Moq.Mock<AutoMapper.IMapper>().Object' in content:
        # We need to insert mapper_setup at the beginning of the constructor
        # and replace the mock with 'mapper'
        
        # Find constructor
        class_name = os.path.basename(file_path).split('.')[0]
        constructor_sig = f"public {class_name}()"
        
        if constructor_sig in content:
            parts = content.split(constructor_sig)
            # Find the first '{' after constructor_sig
            body_start = parts[1].find('{') + 1
            
            new_content = parts[0] + constructor_sig + parts[1][:body_start] + mapper_setup + parts[1][body_start:]
            new_content = new_content.replace('new Moq.Mock<AutoMapper.IMapper>().Object', 'mapper')
            
            with open(file_path, 'w') as f:
                f.write(new_content)
                print(f"Fixed {file_path}")

