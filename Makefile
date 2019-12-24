init-tools:
	docker-compose run --rm devenv sh ./eng/init-tools.sh

gen-code:
	docker-compose run --rm devenv sh ./eng/generate-code.sh

test:
	docker-compose run --rm devenv sh ./eng/run-test.sh

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./publish

.PHONY: all test clean
