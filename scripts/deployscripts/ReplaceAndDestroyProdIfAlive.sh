if python3 MinitwitAlive.py $1 $2; then
        echo staging droplet $1 alive 
        if python3 reassign_floating_ip.py $1 $2; then 
            echo reassign_floating_ip successful
            echo Destroying production droplet $3
            cd ../../aspnet/
            export DROPLET_NAME=$3
            vagrant destroy -f
            if python3 rename_droplet.py $1 $3 $2; then
                echo rename of droplet $1 to $3 successful
            else 
                echo rename of droplet $1 failed
            fi
        else 
            echo staging droplet $1 not found
            cd ../../aspnet/
            export DROPLET_NAME=$1
            vagrant destroy -f
        fi
else
    echo staging droplet $1 not alive
    cd ../../aspnet/
    export DROPLET_NAME=$1
    vagrant destroy -f
fi