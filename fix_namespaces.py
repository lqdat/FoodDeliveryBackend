import os

target_dir = r"d:\DeliverFood\FoodDeliveryBackend\FoodDeliveryBackend.Core\Entities"
old_ns = "namespace FoodDeliveryBackend.Infrastructure.Scaffolding;"
new_ns = "namespace FoodDeliveryBackend.Core.Entities;"

for filename in os.listdir(target_dir):
    if filename.endswith(".cs"):
        path = os.path.join(target_dir, filename)
        with open(path, "r", encoding="utf-8") as f:
            content = f.read()
        
        if old_ns in content:
            new_content = content.replace(old_ns, new_ns)
            with open(path, "w", encoding="utf-8") as f:
                f.write(new_content)
            print(f"Updated {filename}")
        else:
            print(f"Skipped {filename} (namespace not found)")
