# Export env variables (set ELK_DIR to directory where this script is located)
export ELK_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
export ELK_USER="$( whoami )"

echo "export ELK_DIR=$ELK_DIR" >> ~/.profile
echo "export ELK_USER=$ELK_USER" >> ~/.profile

# Go to ELK stack folder
cd $ELK_DIR

# Change permissions on filebat config
sudo chown root filebeat.yml 
sudo chmod go-w filebeat.yml